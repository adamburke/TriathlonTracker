using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using System.Xml.Linq;
using TriathlonTracker.Data;
using TriathlonTracker.Models;
using TriathlonTracker.Models.Enums;
using TriathlonTracker.Services.Interfaces;

namespace TriathlonTracker.Services
{
    public class ComplianceService : IComplianceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComplianceService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ComplianceService(
            ApplicationDbContext context,
            ILogger<ComplianceService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<GdprComplianceMetrics> GetComplianceMetricsAsync()
        {
            try
            {
                // Get admin role ID to exclude admin users from compliance metrics
                var adminRoleId = await _context.Roles
                    .Where(r => r.Name == "Admin")
                    .Select(r => r.Id)
                    .FirstOrDefaultAsync();

                // Get admin user IDs to exclude from counts
                var adminUserIds = adminRoleId != null
                    ? await _context.UserRoles
                        .Where(ur => ur.RoleId == adminRoleId)
                        .Select(ur => ur.UserId)
                        .ToListAsync()
                    : new List<string>();

                // Count only non-admin users for compliance metrics
                var totalUsers = await _context.Users
                    .Where(u => !adminUserIds.Contains(u.Id))
                    .CountAsync();
                
                var usersWithConsent = await _context.Users
                    .Where(u => !adminUserIds.Contains(u.Id) && u.HasGivenConsent)
                    .CountAsync();
                
                var pendingDataRequests = await _context.DataExportRequests.CountAsync(r => r.Status == "Pending") +
                                         await _context.DataRectificationRequests.CountAsync(r => r.Status == "Pending") +
                                         await _context.AccountDeletionRequests.CountAsync(r => r.Status == "Pending");
                var completedDataRequests = await _context.DataExportRequests.CountAsync(r => r.Status == "Completed") +
                                           await _context.DataRectificationRequests.CountAsync(r => r.Status == "Completed") +
                                           await _context.AccountDeletionRequests.CountAsync(r => r.Status == "Completed");
                var activeBreachIncidents = await _context.BreachIncidents.CountAsync(b => b.Status == "Open" || b.Status == "InProgress");
                var resolvedBreachIncidents = await _context.BreachIncidents.CountAsync(b => b.Status == "Resolved");

                return new GdprComplianceMetrics
                {
                    TotalUsers = totalUsers,
                    UsersWithConsent = usersWithConsent,
                    PendingDataRequests = pendingDataRequests,
                    CompletedDataRequests = completedDataRequests,
                    ActiveBreachIncidents = activeBreachIncidents,
                    ResolvedBreachIncidents = resolvedBreachIncidents,
                    DataRetentionViolations = 0, // TODO: Implement retention violation detection
                    LastComplianceCheck = DateTime.UtcNow.AddHours(-1) // TODO: Get from actual compliance check
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compliance metrics");
                throw;
            }
        }

        public Task<bool> RunComplianceCheckAsync()
        {
            try
            {
                // TODO: Implement comprehensive compliance check
                _logger.LogInformation("Compliance check completed at {Time}", DateTime.UtcNow);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running compliance check");
                return Task.FromResult(false);
            }
        }

        public Task<bool> CreateComplianceAlertAsync(string type, string title, string description, string severity)
        {
            try
            {
                // TODO: Implement alert creation
                _logger.LogInformation("Compliance alert created: {Title}", title);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating compliance alert");
                return Task.FromResult(false);
            }
        }

        public Task<bool> ResolveAlertAsync(string alertId, string resolutionNotes)
        {
            try
            {
                // TODO: Implement alert resolution
                _logger.LogInformation("Alert {AlertId} resolved", alertId);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving alert {AlertId}", alertId);
                return Task.FromResult(false);
            }
        }

        public async Task<Dictionary<string, object>> GetComplianceStatusAsync()
        {
            try
            {
                var metrics = await GetComplianceMetricsAsync();
                return new Dictionary<string, object>
                {
                    ["totalUsers"] = metrics.TotalUsers,
                    ["usersWithConsent"] = metrics.UsersWithConsent,
                    ["complianceScore"] = metrics.TotalUsers > 0 ? (double)metrics.UsersWithConsent / metrics.TotalUsers * 100 : 0,
                    ["pendingRequests"] = metrics.PendingDataRequests,
                    ["activeBreaches"] = metrics.ActiveBreachIncidents
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting compliance status");
                throw;
            }
        }

        public Task<ComplianceReport> GenerateComplianceReportAsync(string reportType, DateTime startDate, DateTime endDate)
        {
            try
            {
                var report = new ComplianceReport
                {
                    ReportType = reportType,
                    Title = $"{reportType} Report - {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                    StartDate = startDate,
                    EndDate = endDate,
                    GeneratedAt = DateTime.UtcNow,
                    GeneratedBy = GetCurrentUserId(),
                    Status = "Generated"
                };

                // TODO: Implement actual report generation
                return Task.FromResult(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating compliance report");
                throw;
            }
        }

        public Task<byte[]> ExportComplianceReportAsync(string reportId, string format)
        {
            try
            {
                // TODO: Implement report export
                return Task.FromResult(Array.Empty<byte>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting compliance report");
                throw;
            }
        }

        public Task<List<ComplianceReport>> GetReportsAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                // TODO: Implement report retrieval
                return Task.FromResult(new List<ComplianceReport>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reports");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetConsentAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var consents = await _context.ConsentRecords
                    .Where(c => c.ConsentDate >= startDate && c.ConsentDate <= endDate)
                    .GroupBy(c => c.ConsentType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToListAsync();

                return new Dictionary<string, object>
                {
                    ["consentTypes"] = consents.ToDictionary(c => c.Type, c => c.Count),
                    ["totalConsents"] = consents.Sum(c => c.Count),
                    ["period"] = new { startDate, endDate }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consent analytics");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetDataProcessingAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var processing = await _context.DataProcessingLogs
                    .Where(d => d.ProcessedAt >= startDate && d.ProcessedAt <= endDate)
                    .GroupBy(d => d.Action)
                    .Select(g => new { Action = g.Key, Count = g.Count() })
                    .ToListAsync();

                return new Dictionary<string, object>
                {
                    ["processingActions"] = processing.ToDictionary(p => p.Action, p => p.Count),
                    ["totalProcessing"] = processing.Sum(p => p.Count),
                    ["period"] = new { startDate, endDate }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data processing analytics");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetBreachAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var breaches = await _context.BreachIncidents
                    .Where(b => b.DetectedAt >= startDate && b.DetectedAt <= endDate)
                    .GroupBy(b => b.Severity)
                    .Select(g => new { Severity = g.Key, Count = g.Count() })
                    .ToListAsync();

                return new Dictionary<string, object>
                {
                    ["breachSeverities"] = breaches.ToDictionary(b => b.Severity, b => b.Count),
                    ["totalBreaches"] = breaches.Sum(b => b.Count),
                    ["period"] = new { startDate, endDate }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting breach analytics");
                throw;
            }
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "System";
        }
    }
} 