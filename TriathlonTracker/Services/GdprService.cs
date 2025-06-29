using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TriathlonTracker.Data;
using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public class GdprService : IGdprService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GdprService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GdprService(ApplicationDbContext context, ILogger<GdprService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> ExportUserDataAsync(string userId, string format = "JSON")
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.ConsentRecords)
                    .Include(u => u.DataProcessingLogs)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    throw new ArgumentException("User not found", nameof(userId));

                var triathlons = await _context.Triathlons
                    .Where(t => t.UserId == userId)
                    .ToListAsync();

                var exportData = new
                {
                    PersonalData = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.PhoneNumber,
                        user.CreatedAt,
                        user.ConsentDate,
                        user.ConsentVersion,
                        user.HasGivenConsent,
                        user.MarketingConsent,
                        user.AnalyticsConsent,
                        user.DataRetentionPreference
                    },
                    TriathlonData = triathlons.Select(t => new
                    {
                        t.Id,
                        t.RaceName,
                        t.RaceDate,
                        t.Location,
                        t.SwimDistance,
                        t.SwimUnit,
                        t.SwimTime,
                        t.BikeDistance,
                        t.BikeUnit,
                        t.BikeTime,
                        t.RunDistance,
                        t.RunUnit,
                        t.RunTime,
                        t.CreatedAt,
                        t.UpdatedAt
                    }),
                    ConsentHistory = user.ConsentRecords.Select(c => new
                    {
                        c.ConsentType,
                        c.IsGranted,
                        c.ConsentDate,
                        c.WithdrawnDate,
                        c.Purpose,
                        c.LegalBasis,
                        c.ConsentVersion
                    }),
                    ProcessingHistory = user.DataProcessingLogs.Select(l => new
                    {
                        l.Action,
                        l.DataType,
                        l.Purpose,
                        l.LegalBasis,
                        l.ProcessedAt,
                        l.ProcessedBy
                    }),
                    ExportMetadata = new
                    {
                        ExportDate = DateTime.UtcNow,
                        Format = format,
                        RequestedBy = userId
                    }
                };

                await LogDataProcessingAsync(userId, "DataExported", "AllPersonalData", "UserRequest", "Consent", $"Format: {format}");

                // Update last export date
                user.LastDataExportDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting user data for user {UserId}", userId);
                throw;
            }
        }

        public async Task<byte[]> ExportUserDataAsPdfAsync(string userId)
        {
            // For now, return JSON as bytes - PDF generation would require additional libraries
            var jsonData = await ExportUserDataAsync(userId, "PDF");
            return System.Text.Encoding.UTF8.GetBytes(jsonData);
        }

        public async Task<bool> RequestAccountDeletionAsync(string userId, string reason = "")
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                user.IsAccountDeletionRequested = true;
                user.AccountDeletionRequestDate = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                await LogDataProcessingAsync(userId, "DeletionRequested", "AccountData", "UserRequest", "Consent", reason);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Account deletion requested for user {UserId}. Reason: {Reason}", userId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting account deletion for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ProcessAccountDeletionAsync(string userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.ConsentRecords)
                    .Include(u => u.DataProcessingLogs)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return false;

                // Delete related triathlon data
                var triathlons = await _context.Triathlons.Where(t => t.UserId == userId).ToListAsync();
                _context.Triathlons.RemoveRange(triathlons);

                // Log the deletion before removing logs
                await LogDataProcessingAsync(userId, "AccountDeleted", "AllPersonalData", "UserRequest", "Consent", "Account deletion processed");

                // Remove consent records and processing logs
                _context.ConsentRecords.RemoveRange(user.ConsentRecords);
                _context.DataProcessingLogs.RemoveRange(user.DataProcessingLogs);

                // Remove user account
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Account deletion processed for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing account deletion for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> AnonymizeUserDataAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return false;

                // Anonymize personal data
                user.FirstName = "Anonymized";
                user.LastName = "User";
                user.Email = $"anonymized_{Guid.NewGuid():N}@example.com";
                user.UserName = user.Email;
                user.PhoneNumber = null;
                user.UpdatedAt = DateTime.UtcNow;

                await LogDataProcessingAsync(userId, "DataAnonymized", "PersonalData", "DataRetention", "LegitimateInterest", "User data anonymized");
                await _context.SaveChangesAsync();

                _logger.LogInformation("User data anonymized for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anonymizing user data for user {UserId}", userId);
                return false;
            }
        }

        public async Task LogDataProcessingAsync(string userId, string action, string dataType, string purpose, string legalBasis, string? additionalData = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var log = new DataProcessingLog
                {
                    UserId = userId,
                    Action = action,
                    DataType = dataType,
                    Purpose = purpose,
                    LegalBasis = legalBasis,
                    Description = $"{action} performed on {dataType}",
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown",
                    ProcessedBy = httpContext?.User?.Identity?.Name ?? "System",
                    IsAutomated = httpContext?.User?.Identity?.Name == null,
                    AdditionalData = additionalData ?? "",
                    ProcessedAt = DateTime.UtcNow
                };

                _context.DataProcessingLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging data processing for user {UserId}", userId);
                // Don't throw here to avoid breaking the main operation
            }
        }

        public async Task<IEnumerable<DataProcessingLog>> GetDataProcessingLogsAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.DataProcessingLogs.Where(l => l.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(l => l.ProcessedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(l => l.ProcessedAt <= toDate.Value);

            return await query.OrderByDescending(l => l.ProcessedAt).ToListAsync();
        }

        public async Task<bool> ApplyDataRetentionPoliciesAsync()
        {
            try
            {
                var policies = await _context.DataRetentionPolicies
                    .Where(p => p.IsActive && p.AutoDelete)
                    .ToListAsync();

                foreach (var policy in policies)
                {
                    var cutoffDate = DateTime.UtcNow.AddDays(-policy.RetentionPeriodDays);

                    switch (policy.DataType.ToLower())
                    {
                        case "processinglogs":
                            var expiredLogs = await _context.DataProcessingLogs
                                .Where(l => l.ProcessedAt < cutoffDate)
                                .ToListAsync();
                            _context.DataProcessingLogs.RemoveRange(expiredLogs);
                            break;

                        case "consentrecords":
                            var expiredConsents = await _context.ConsentRecords
                                .Where(c => c.CreatedAt < cutoffDate && c.WithdrawnDate.HasValue)
                                .ToListAsync();
                            _context.ConsentRecords.RemoveRange(expiredConsents);
                            break;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying data retention policies");
                return false;
            }
        }

        public async Task<IEnumerable<DataRetentionPolicy>> GetDataRetentionPoliciesAsync()
        {
            return await _context.DataRetentionPolicies
                .Where(p => p.IsActive)
                .OrderBy(p => p.DataType)
                .ToListAsync();
        }

        public async Task<bool> IsDataExpiredAsync(string dataType, DateTime createdDate)
        {
            var policy = await _context.DataRetentionPolicies
                .FirstOrDefaultAsync(p => p.DataType.ToLower() == dataType.ToLower() && p.IsActive);

            if (policy == null)
                return false;

            var expirationDate = createdDate.AddDays(policy.RetentionPeriodDays);
            return DateTime.UtcNow > expirationDate;
        }

        public async Task<bool> HasUserGivenConsentAsync(string userId, string consentType)
        {
            var latestConsent = await _context.ConsentRecords
                .Where(c => c.UserId == userId && c.ConsentType == consentType)
                .OrderByDescending(c => c.ConsentDate)
                .FirstOrDefaultAsync();

            return latestConsent?.IsGranted == true && !latestConsent.WithdrawnDate.HasValue;
        }

        public async Task<DateTime?> GetLastDataExportDateAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.LastDataExportDate;
        }

        public async Task<bool> IsAccountDeletionRequestedAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user?.IsAccountDeletionRequested == true;
        }

        public async Task<object> GenerateComplianceReportAsync(DateTime fromDate, DateTime toDate)
        {
            var totalUsers = await _context.Users.CountAsync();
            var usersWithConsent = await _context.Users.CountAsync(u => u.HasGivenConsent);
            var dataExports = await _context.DataProcessingLogs
                .CountAsync(l => l.Action == "DataExported" && l.ProcessedAt >= fromDate && l.ProcessedAt <= toDate);
            var deletionRequests = await _context.Users
                .CountAsync(u => u.IsAccountDeletionRequested && u.AccountDeletionRequestDate >= fromDate && u.AccountDeletionRequestDate <= toDate);

            return new
            {
                ReportPeriod = new { FromDate = fromDate, ToDate = toDate },
                UserStatistics = new
                {
                    TotalUsers = totalUsers,
                    UsersWithConsent = usersWithConsent,
                    ConsentRate = totalUsers > 0 ? (double)usersWithConsent / totalUsers * 100 : 0
                },
                DataRequests = new
                {
                    DataExports = dataExports,
                    DeletionRequests = deletionRequests
                },
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<string>> GetExpiredDataTypesAsync()
        {
            var policies = await GetDataRetentionPoliciesAsync();
            var expiredTypes = new List<string>();

            foreach (var policy in policies)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-policy.RetentionPeriodDays);
                var hasExpiredData = false;

                switch (policy.DataType.ToLower())
                {
                    case "processinglogs":
                        hasExpiredData = await _context.DataProcessingLogs.AnyAsync(l => l.ProcessedAt < cutoffDate);
                        break;
                    case "consentrecords":
                        hasExpiredData = await _context.ConsentRecords.AnyAsync(c => c.CreatedAt < cutoffDate);
                        break;
                }

                if (hasExpiredData)
                    expiredTypes.Add(policy.DataType);
            }

            return expiredTypes;
        }

        public async Task<bool> ValidateDataExportRequestAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Data export is a fundamental GDPR right, so don't require consent
            // However, still check for rate limiting to prevent abuse
            if (user.LastDataExportDate.HasValue)
            {
                var daysSinceLastExport = (DateTime.UtcNow - user.LastDataExportDate.Value).TotalDays;
                if (daysSinceLastExport < 1) // Limit to one export per day
                    return false;
            }

            return true;
        }

        public async Task<string> GetUserDataSummaryAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return "User not found";

            var triathlonCount = await _context.Triathlons.CountAsync(t => t.UserId == userId);
            var consentCount = await _context.ConsentRecords.CountAsync(c => c.UserId == userId);
            var logCount = await _context.DataProcessingLogs.CountAsync(l => l.UserId == userId);

            return $"User has {triathlonCount} triathlon records, {consentCount} consent records, and {logCount} processing log entries.";
        }
    }
}