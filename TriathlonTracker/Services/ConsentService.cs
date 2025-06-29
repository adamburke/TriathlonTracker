using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public class ConsentService : IConsentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConsentService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGdprService _gdprService;

        public ConsentService(ApplicationDbContext context, ILogger<ConsentService> logger, IHttpContextAccessor httpContextAccessor, IGdprService gdprService)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _gdprService = gdprService;
        }

        public async Task<bool> GrantConsentAsync(string userId, string consentType, string purpose, string legalBasis, string consentVersion = "1.0")
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                
                var consentRecord = new ConsentRecord
                {
                    UserId = userId,
                    ConsentType = consentType,
                    IsGranted = true,
                    ConsentDate = DateTime.UtcNow,
                    Purpose = purpose,
                    LegalBasis = legalBasis,
                    ConsentVersion = consentVersion,
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
                };

                _context.ConsentRecords.Add(consentRecord);

                // Update user consent flags
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.HasGivenConsent = true;
                    user.ConsentDate = DateTime.UtcNow;
                    user.ConsentVersion = consentVersion;
                    user.UpdatedAt = DateTime.UtcNow;

                    // Update specific consent types
                    switch (consentType.ToLower())
                    {
                        case "marketing":
                            user.MarketingConsent = true;
                            break;
                        case "analytics":
                            user.AnalyticsConsent = true;
                            break;
                    }
                }

                await _context.SaveChangesAsync();

                await _gdprService.LogDataProcessingAsync(userId, "ConsentGranted", "ConsentData", purpose, legalBasis, $"ConsentType: {consentType}");

                _logger.LogInformation("Consent granted for user {UserId}, type {ConsentType}", userId, consentType);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error granting consent for user {UserId}, type {ConsentType}", userId, consentType);
                return false;
            }
        }

        public async Task<bool> WithdrawConsentAsync(string userId, string consentType)
        {
            try
            {
                var latestConsent = await GetLatestConsentAsync(userId, consentType);
                if (latestConsent == null || !latestConsent.IsGranted)
                    return false;

                // Create withdrawal record
                var withdrawalRecord = new ConsentRecord
                {
                    UserId = userId,
                    ConsentType = consentType,
                    IsGranted = false,
                    ConsentDate = DateTime.UtcNow,
                    WithdrawnDate = DateTime.UtcNow,
                    Purpose = latestConsent.Purpose,
                    LegalBasis = latestConsent.LegalBasis,
                    ConsentVersion = latestConsent.ConsentVersion,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
                };

                _context.ConsentRecords.Add(withdrawalRecord);

                // Update user consent flags
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.ConsentWithdrawnDate = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;

                    // Update specific consent types
                    switch (consentType.ToLower())
                    {
                        case "marketing":
                            user.MarketingConsent = false;
                            break;
                        case "analytics":
                            user.AnalyticsConsent = false;
                            break;
                        case "dataprocessing":
                            user.HasGivenConsent = false;
                            break;
                    }
                }

                await _context.SaveChangesAsync();

                await _gdprService.LogDataProcessingAsync(userId, "ConsentWithdrawn", "ConsentData", "UserRequest", "Consent", $"ConsentType: {consentType}");

                _logger.LogInformation("Consent withdrawn for user {UserId}, type {ConsentType}", userId, consentType);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing consent for user {UserId}, type {ConsentType}", userId, consentType);
                return false;
            }
        }

        public async Task<bool> UpdateConsentAsync(string userId, string consentType, bool isGranted, string? reason = null)
        {
            if (isGranted)
            {
                return await GrantConsentAsync(userId, consentType, reason ?? "User consent update", "Consent");
            }
            else
            {
                return await WithdrawConsentAsync(userId, consentType);
            }
        }

        public async Task<bool> HasValidConsentAsync(string userId, string consentType)
        {
            var latestConsent = await GetLatestConsentAsync(userId, consentType);
            return latestConsent?.IsGranted == true && !latestConsent.WithdrawnDate.HasValue;
        }

        public async Task<ConsentRecord?> GetLatestConsentAsync(string userId, string consentType)
        {
            return await _context.ConsentRecords
                .Where(c => c.UserId == userId && c.ConsentType == consentType)
                .OrderByDescending(c => c.ConsentDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ConsentRecord>> GetConsentHistoryAsync(string userId)
        {
            return await _context.ConsentRecords
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.ConsentDate)
                .ToListAsync();
        }

        public async Task<bool> HasDataProcessingConsentAsync(string userId)
        {
            return await HasValidConsentAsync(userId, "DataProcessing");
        }

        public async Task<bool> HasMarketingConsentAsync(string userId)
        {
            return await HasValidConsentAsync(userId, "Marketing");
        }

        public async Task<bool> HasAnalyticsConsentAsync(string userId)
        {
            return await HasValidConsentAsync(userId, "Analytics");
        }

        public Task<bool> IsConsentRequiredAsync(string consentType)
        {
            // Define which consent types are required
            var requiredConsents = new[] { "DataProcessing", "Essential" };
            return Task.FromResult(requiredConsents.Contains(consentType, StringComparer.OrdinalIgnoreCase));
        }

        public async Task<bool> IsConsentValidAsync(string userId, string consentType)
        {
            var consent = await GetLatestConsentAsync(userId, consentType);
            if (consent == null || !consent.IsGranted || consent.WithdrawnDate.HasValue)
                return false;

            // Check if consent has expired (example: 2 years for GDPR)
            var expirationDate = await GetConsentExpirationDateAsync(userId, consentType);
            if (expirationDate.HasValue && DateTime.UtcNow > expirationDate.Value)
                return false;

            return true;
        }

        public async Task<DateTime?> GetConsentExpirationDateAsync(string userId, string consentType)
        {
            var consent = await GetLatestConsentAsync(userId, consentType);
            if (consent == null)
                return null;

            // GDPR doesn't specify expiration, but best practice is to renew consent periodically
            // Different consent types may have different expiration periods
            var expirationMonths = consentType.ToLower() switch
            {
                "marketing" => 24, // 2 years
                "analytics" => 12, // 1 year
                "dataprocessing" => 36, // 3 years
                _ => 24 // Default 2 years
            };

            return consent.ConsentDate.AddMonths(expirationMonths);
        }

        public async Task<object> GetConsentSummaryAsync(string userId)
        {
            var consents = await GetConsentHistoryAsync(userId);
            var consentsByType = consents.GroupBy(c => c.ConsentType).ToList();

            var summary = new
            {
                UserId = userId,
                TotalConsentRecords = consents.Count(),
                ConsentTypes = consentsByType.Select(g => new
                {
                    ConsentType = g.Key,
                    CurrentStatus = g.OrderByDescending(c => c.ConsentDate).First().IsGranted,
                    LastUpdated = g.OrderByDescending(c => c.ConsentDate).First().ConsentDate,
                    TotalChanges = g.Count()
                }),
                HasDataProcessingConsent = await HasDataProcessingConsentAsync(userId),
                HasMarketingConsent = await HasMarketingConsentAsync(userId),
                HasAnalyticsConsent = await HasAnalyticsConsentAsync(userId)
            };

            return summary;
        }

        public async Task<IEnumerable<ConsentRecord>> GetExpiredConsentsAsync()
        {
            var cutoffDate = DateTime.UtcNow.AddMonths(-24); // Consider consents older than 2 years as expired
            return await _context.ConsentRecords
                .Where(c => c.IsGranted && !c.WithdrawnDate.HasValue && c.ConsentDate < cutoffDate)
                .ToListAsync();
        }

        public async Task<int> GetConsentRateAsync(string consentType)
        {
            var totalUsers = await _context.Users.CountAsync();
            if (totalUsers == 0)
                return 0;

            var usersWithConsent = await _context.ConsentRecords
                .Where(c => c.ConsentType == consentType && c.IsGranted && !c.WithdrawnDate.HasValue)
                .Select(c => c.UserId)
                .Distinct()
                .CountAsync();

            return (int)Math.Round((double)usersWithConsent / totalUsers * 100);
        }

        public async Task<bool> IsConsentRenewalRequiredAsync(string userId, string consentType)
        {
            var expirationDate = await GetConsentExpirationDateAsync(userId, consentType);
            if (!expirationDate.HasValue)
                return false;

            // Require renewal 30 days before expiration
            var renewalDate = expirationDate.Value.AddDays(-30);
            return DateTime.UtcNow >= renewalDate;
        }

        public async Task<bool> RenewConsentAsync(string userId, string consentType, string newVersion)
        {
            var currentConsent = await GetLatestConsentAsync(userId, consentType);
            if (currentConsent == null)
                return false;

            return await GrantConsentAsync(userId, consentType, currentConsent.Purpose, currentConsent.LegalBasis, newVersion);
        }

        public async Task<bool> BulkWithdrawConsentAsync(string userId, IEnumerable<string> consentTypes)
        {
            try
            {
                var success = true;
                foreach (var consentType in consentTypes)
                {
                    var result = await WithdrawConsentAsync(userId, consentType);
                    if (!result)
                        success = false;
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk consent withdrawal for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> MigrateConsentToNewVersionAsync(string oldVersion, string newVersion)
        {
            try
            {
                var consentsToMigrate = await _context.ConsentRecords
                    .Where(c => c.ConsentVersion == oldVersion && c.IsGranted && !c.WithdrawnDate.HasValue)
                    .ToListAsync();

                foreach (var consent in consentsToMigrate)
                {
                    // Create new consent record with new version
                    await GrantConsentAsync(consent.UserId, consent.ConsentType, consent.Purpose, consent.LegalBasis, newVersion);
                }

                _logger.LogInformation("Migrated {Count} consent records from version {OldVersion} to {NewVersion}", 
                    consentsToMigrate.Count, oldVersion, newVersion);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating consent from version {OldVersion} to {NewVersion}", oldVersion, newVersion);
                return false;
            }
        }
    }
}