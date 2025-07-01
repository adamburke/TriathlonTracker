using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;
using TriathlonTracker.Models.Enums;
using System.Text.Json;

namespace TriathlonTracker.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminDashboardService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminDashboardService(
            ApplicationDbContext context,
            ILogger<AdminDashboardService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AdminDashboardViewModel> GetDashboardDataAsync()
        {
            var dashboard = new AdminDashboardViewModel
            {
                ComplianceMetrics = await GetComplianceMetricsAsync(),
                UserStatuses = await GetUserGdprStatusesAsync(1, 10),
                RecentActivities = await GetRecentActivitiesAsync(10),
                Alerts = await GetActiveAlertsAsync(),
                RetentionSummary = await GetRetentionSummaryAsync()
            };

            return dashboard;
        }

        public async Task<GdprComplianceMetrics> GetComplianceMetricsAsync()
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

        public async Task<List<UserGdprStatus>> GetUserGdprStatusesAsync(int page = 1, int pageSize = 50)
        {
            var users = await _context.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserGdprStatus
                {
                    UserId = u.Id,
                    Email = u.Email ?? "",
                    Name = $"{u.FirstName} {u.LastName}",
                    HasConsent = u.HasGivenConsent,
                    ConsentDate = u.ConsentDate,
                    LastDataAccess = u.LastDataExportDate,
                    PendingRequests = 0, // TODO: Calculate pending requests
                    HasRetentionViolation = false, // TODO: Check retention violations
                    CreatedDate = u.CreatedAt
                })
                .ToListAsync();

            return users;
        }

        public async Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 20)
        {
            var activities = new List<RecentActivity>();

            // Get recent data processing logs
            var recentLogs = await _context.DataProcessingLogs
                .OrderByDescending(l => l.ProcessedAt)
                .Take(count / 2)
                .Include(l => l.User)
                .ToListAsync();

            activities.AddRange(recentLogs.Select(l => new RecentActivity
            {
                Type = "DataProcessing",
                Description = $"{l.Action} - {l.DataType}",
                UserId = l.UserId,
                UserEmail = l.User?.Email ?? "Unknown",
                Timestamp = l.ProcessedAt,
                Status = "Completed"
            }));

            // Get recent consent records
            var recentConsents = await _context.ConsentRecords
                .OrderByDescending(c => c.ConsentDate)
                .Take(count / 2)
                .Include(c => c.User)
                .ToListAsync();

            activities.AddRange(recentConsents.Select(c => new RecentActivity
            {
                Type = "Consent",
                Description = $"Consent {(c.IsGranted ? "granted" : "withdrawn")} for {c.ConsentType}",
                UserId = c.UserId,
                UserEmail = c.User?.Email ?? "Unknown",
                Timestamp = c.ConsentDate,
                Status = c.IsGranted ? "Granted" : "Withdrawn"
            }));

            return activities.OrderByDescending(a => a.Timestamp).Take(count).ToList();
        }

        public async Task<List<ComplianceAlert>> GetActiveAlertsAsync()
        {
            var alerts = new List<ComplianceAlert>();

            // Check for users without consent
            var usersWithoutConsent = await _context.Users.CountAsync(u => !u.HasGivenConsent);
            if (usersWithoutConsent > 0)
            {
                alerts.Add(new ComplianceAlert
                {
                    Type = "ConsentCompliance",
                    Title = "Users Without Consent",
                    Description = $"{usersWithoutConsent} users have not provided consent",
                    Severity = "Medium",
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Check for pending data requests
            var pendingRequests = await _context.DataExportRequests.CountAsync(r => r.Status == "Pending");
            if (pendingRequests > 0)
            {
                alerts.Add(new ComplianceAlert
                {
                    Type = "DataRequests",
                    Title = "Pending Data Requests",
                    Description = $"{pendingRequests} data export requests are pending",
                    Severity = "High",
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Check for active breach incidents
            var activeBreaches = await _context.BreachIncidents.CountAsync(b => b.Status != "Resolved");
            if (activeBreaches > 0)
            {
                alerts.Add(new ComplianceAlert
                {
                    Type = "BreachIncident",
                    Title = "Active Breach Incidents",
                    Description = $"{activeBreaches} breach incidents require attention",
                    Severity = "Critical",
                    CreatedDate = DateTime.UtcNow
                });
            }

            return alerts;
        }

        public async Task<DataRetentionSummary> GetRetentionSummaryAsync()
        {
            var totalRecords = await _context.DataProcessingLogs.CountAsync() +
                              await _context.ConsentRecords.CountAsync() +
                              await _context.GdprAuditLogs.CountAsync();

            var recordsNearExpiry = await _context.DataProcessingLogs
                .Where(l => l.ProcessedAt < DateTime.UtcNow.AddDays(-365))
                .CountAsync();

            var expiredRecords = await _context.DataProcessingLogs
                .Where(l => l.ProcessedAt < DateTime.UtcNow.AddDays(-730))
                .CountAsync();

            var archivedRecords = 0; // TODO: Implement archived records count

            var nextCleanupDate = DateTime.UtcNow.AddDays(7); // TODO: Calculate based on retention policies

            var policyStatuses = new List<RetentionPolicyStatus>
            {
                new RetentionPolicyStatus
                {
                    PolicyName = "Data Processing Logs",
                    AffectedRecords = await _context.DataProcessingLogs.CountAsync(),
                    LastExecuted = DateTime.UtcNow.AddDays(-1),
                    Status = "Completed"
                },
                new RetentionPolicyStatus
                {
                    PolicyName = "Consent Records",
                    AffectedRecords = await _context.ConsentRecords.CountAsync(),
                    LastExecuted = DateTime.UtcNow.AddDays(-2),
                    Status = "Completed"
                }
            };

            return new DataRetentionSummary
            {
                TotalRecords = totalRecords,
                RecordsNearExpiry = recordsNearExpiry,
                ExpiredRecords = expiredRecords,
                ArchivedRecords = archivedRecords,
                NextCleanupDate = nextCleanupDate,
                PolicyStatuses = policyStatuses
            };
        }

        public async Task<UserGdprStatus?> GetUserGdprStatusAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var pendingRequests = await _context.DataExportRequests.CountAsync(r => r.UserId == userId && r.Status == "Pending") +
                                 await _context.DataRectificationRequests.CountAsync(r => r.UserId == userId && r.Status == "Pending") +
                                 await _context.AccountDeletionRequests.CountAsync(r => r.UserId == userId && r.Status == "Pending");

            return new UserGdprStatus
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                Name = $"{user.FirstName} {user.LastName}",
                HasConsent = user.HasGivenConsent,
                ConsentDate = user.ConsentDate,
                LastDataAccess = user.LastDataExportDate,
                PendingRequests = pendingRequests,
                HasRetentionViolation = false, // TODO: Check retention violations
                CreatedDate = user.CreatedAt
            };
        }

        public async Task<bool> UpdateUserConsentAsync(string userId, string consentType, bool isGranted, string reason)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                var consentRecord = new ConsentRecord
                {
                    UserId = userId,
                    ConsentType = consentType,
                    IsGranted = isGranted,
                    ConsentDate = DateTime.UtcNow,
                    Purpose = reason,
                    LegalBasis = "AdminUpdate",
                    ConsentVersion = "1.0",
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent()
                };

                _context.ConsentRecords.Add(consentRecord);

                // Update user's general consent status if this is a general consent
                if (consentType == "DataProcessing")
                {
                    user.HasGivenConsent = isGranted;
                    user.ConsentDate = isGranted ? DateTime.UtcNow : null;
                    user.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user consent for {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UpdateUserDetailsAsync(string userId, string firstName, string lastName, string email, bool hasConsent)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                user.FirstName = firstName;
                user.LastName = lastName;
                user.Email = email;
                user.HasGivenConsent = hasConsent;
                user.UpdatedAt = DateTime.UtcNow;

                if (hasConsent && !user.ConsentDate.HasValue)
                {
                    user.ConsentDate = DateTime.UtcNow;
                }
                else if (!hasConsent)
                {
                    user.ConsentDate = null;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user details for {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> BulkUpdateConsentAsync(BulkConsentOperation operation)
        {
            try
            {
                foreach (var userId in operation.UserIds)
                {
                    await UpdateUserConsentAsync(userId, operation.ConsentType ?? "DataProcessing", 
                        operation.Operation == "Grant", operation.Reason ?? "Bulk operation");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk consent operation");
                return false;
            }
        }

        public async Task<List<UserGdprStatus>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 50)
        {
            var users = await _context.Users
                .Where(u => u.Email!.Contains(searchTerm) || 
                           u.FirstName.Contains(searchTerm) || 
                           u.LastName.Contains(searchTerm))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserGdprStatus
                {
                    UserId = u.Id,
                    Email = u.Email ?? "",
                    Name = $"{u.FirstName} {u.LastName}",
                    HasConsent = u.HasGivenConsent,
                    ConsentDate = u.ConsentDate,
                    LastDataAccess = u.LastDataExportDate,
                    PendingRequests = 0,
                    HasRetentionViolation = false,
                    CreatedDate = u.CreatedAt
                })
                .ToListAsync();

            return users;
        }

        public async Task<bool> RunComplianceCheckAsync()
        {
            try
            {
                // TODO: Implement comprehensive compliance check
                _logger.LogInformation("Running compliance check");
                await Task.Delay(1000); // Simulate processing
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running compliance check");
                return false;
            }
        }

        public Task<bool> CreateComplianceAlertAsync(string type, string title, string description, string severity)
        {
            try
            {
                var alert = new ComplianceAlert
                {
                    Type = type,
                    Title = title,
                    Description = description,
                    Severity = severity,
                    CreatedDate = DateTime.UtcNow
                };

                // TODO: Store alerts in database when alert model is added to context
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
                // TODO: Implement alert resolution when alerts are stored in database
                _logger.LogInformation("Resolving alert {AlertId}: {Notes}", alertId, resolutionNotes);
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
            var metrics = await GetComplianceMetricsAsync();
            return new Dictionary<string, object>
            {
                ["totalUsers"] = metrics.TotalUsers,
                ["consentRate"] = metrics.ConsentRate,
                ["pendingRequests"] = metrics.PendingDataRequests,
                ["activeBreaches"] = metrics.ActiveBreachIncidents,
                ["lastCheck"] = metrics.LastComplianceCheck
            };
        }

        public Task<bool> ExecuteRetentionPolicyAsync(string policyId)
        {
            try
            {
                // TODO: Implement retention policy execution
                _logger.LogInformation("Executing retention policy {PolicyId}", policyId);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing retention policy {PolicyId}", policyId);
                return Task.FromResult(false);
            }
        }

        public async Task<List<RetentionPolicyStatus>> GetRetentionPolicyStatusesAsync()
        {
            var policies = await _context.DataRetentionPolicies
                .Where(p => p.IsActive)
                .ToListAsync();

            return policies.Select(p => new RetentionPolicyStatus
            {
                PolicyName = p.Description,
                AffectedRecords = 0, // TODO: Calculate
                LastExecuted = DateTime.UtcNow.AddDays(-1),
                Status = "Completed"
            }).ToList();
        }

        public Task<bool> ScheduleDataCleanupAsync(DateTime scheduledDate)
        {
            try
            {
                // TODO: Implement data cleanup scheduling
                _logger.LogInformation("Scheduling data cleanup for {Date}", scheduledDate);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling data cleanup");
                return Task.FromResult(false);
            }
        }

        public Task<List<string>> GetExpiredDataSummaryAsync()
        {
            // TODO: Implement expired data detection
            return Task.FromResult(new List<string>());
        }

        public async Task<ComplianceReport> GenerateComplianceReportAsync(string reportType, DateTime startDate, DateTime endDate)
        {
            var report = new ComplianceReport
            {
                Title = $"{reportType} Compliance Report",
                Type = reportType,
                GeneratedDate = DateTime.UtcNow,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                GeneratedBy = GetCurrentUserId(),
                Data = await GetReportDataAsync(reportType, startDate, endDate)
            };

            return report;
        }

        public Task<byte[]> ExportComplianceReportAsync(string reportId, string format)
        {
            // TODO: Implement report export in various formats
            var reportData = JsonSerializer.Serialize(new { reportId, format, exportedAt = DateTime.UtcNow });
            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(reportData));
        }

        public Task<List<ComplianceReport>> GetReportsAsync(int page = 1, int pageSize = 20)
        {
            // TODO: Implement report storage and retrieval
            return Task.FromResult(new List<ComplianceReport>());
        }

        public async Task<Dictionary<string, object>> GetConsentAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var consents = await _context.ConsentRecords
                .Where(c => c.ConsentDate >= startDate && c.ConsentDate <= endDate)
                .ToListAsync();

            return new Dictionary<string, object>
            {
                ["totalConsents"] = consents.Count,
                ["grantedConsents"] = consents.Count(c => c.IsGranted),
                ["revokedConsents"] = consents.Count(c => !c.IsGranted),
                ["consentsByType"] = consents.GroupBy(c => c.ConsentType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public async Task<Dictionary<string, object>> GetDataProcessingAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await _context.DataProcessingLogs
                .Where(l => l.ProcessedAt >= startDate && l.ProcessedAt <= endDate)
                .ToListAsync();

            return new Dictionary<string, object>
            {
                ["totalActivities"] = logs.Count,
                ["activitiesByType"] = logs.GroupBy(l => l.Action)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ["activitiesByDataType"] = logs.GroupBy(l => l.DataType)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public async Task<Dictionary<string, object>> GetBreachAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var breaches = await _context.BreachIncidents
                .Where(b => b.DetectedDate >= startDate && b.DetectedDate <= endDate)
                .ToListAsync();

            return new Dictionary<string, object>
            {
                ["totalBreaches"] = breaches.Count,
                ["breachesBySeverity"] = breaches.GroupBy(b => b.Severity)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ["breachesByStatus"] = breaches.GroupBy(b => b.Status)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public async Task LogAdminActionAsync(string action, string entityType, string entityId, string details)
        {
            try
            {
                var auditLog = new GdprAuditLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    UserId = entityId,
                    AdminUserId = GetCurrentUserId(),
                    Timestamp = DateTime.UtcNow,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent(),
                    Details = details,
                    IsSuccessful = true
                };

                _context.GdprAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging admin action: {Action}", action);
            }
        }

        public async Task<List<GdprAuditLog>> GetAuditLogsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50)
        {
            var query = _context.GdprAuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.UserId == userId || l.AdminUserId == userId);

            if (startDate.HasValue)
                query = query.Where(l => l.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.Timestamp <= endDate.Value);

            return await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<GdprAuditLog>> SearchAuditLogsAsync(string searchTerm, int page = 1, int pageSize = 50)
        {
            return await _context.GdprAuditLogs
                .Where(l => l.Action.Contains(searchTerm) || 
                           l.EntityType.Contains(searchTerm) ||
                           (l.Details != null && l.Details.Contains(searchTerm)))
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        private async Task<Dictionary<string, object>> GetReportDataAsync(string reportType, DateTime startDate, DateTime endDate)
        {
            return reportType.ToLower() switch
            {
                "consent" => await GetConsentAnalyticsAsync(startDate, endDate),
                "processing" => await GetDataProcessingAnalyticsAsync(startDate, endDate),
                "breach" => await GetBreachAnalyticsAsync(startDate, endDate),
                _ => new Dictionary<string, object>()
            };
        }

        private string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "System";
        }

        private string GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
        }

        public async Task<List<BreachIncident>> GetBreachIncidentsAsync(int count = 50)
        {
            return await _context.BreachIncidents
                .OrderByDescending(b => b.DetectedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<SuspiciousActivity>> GetSuspiciousActivitiesAsync(int count = 50)
        {
            return await _context.SuspiciousActivities
                .OrderByDescending(s => s.DetectedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetRecentAuditLogsAsync(int count = 50)
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found for deletion", userId);
                    return false;
                }

                // Check if user is an admin - prevent deletion of admin users
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();

                if (userRoles.Contains("Admin"))
                {
                    _logger.LogWarning("Attempted to delete admin user {UserId}", userId);
                    return false;
                }

                // Log the deletion action before removing the user
                await LogAdminActionAsync("DeleteUser", "User", userId,
                    $"User {user.Email} ({user.FirstName} {user.LastName}) deleted by admin");

                // Remove related data first (foreign key constraints)
                var consentRecords = _context.ConsentRecords.Where(c => c.UserId == userId);
                _context.ConsentRecords.RemoveRange(consentRecords);

                var processingLogs = _context.DataProcessingLogs.Where(l => l.UserId == userId);
                _context.DataProcessingLogs.RemoveRange(processingLogs);

                var exportRequests = _context.DataExportRequests.Where(r => r.UserId == userId);
                _context.DataExportRequests.RemoveRange(exportRequests);

                var rectificationRequests = _context.DataRectificationRequests.Where(r => r.UserId == userId);
                _context.DataRectificationRequests.RemoveRange(rectificationRequests);

                var deletionRequests = _context.AccountDeletionRequests.Where(r => r.UserId == userId);
                _context.AccountDeletionRequests.RemoveRange(deletionRequests);

                var triathlons = _context.Triathlons.Where(t => t.UserId == userId);
                _context.Triathlons.RemoveRange(triathlons);

                // Finally remove the user
                _context.Users.Remove(user);
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("User {UserId} successfully deleted", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return false;
            }
        }

        public async Task<byte[]> ExportAnalyticsDataAsync(DateTime startDate, DateTime endDate, string format)
        {
            try
            {
                var analytics = new
                {
                    ExportDate = DateTime.UtcNow,
                    PeriodStart = startDate,
                    PeriodEnd = endDate,
                    Format = format,
                    ConsentAnalytics = await GetConsentAnalyticsAsync(startDate, endDate),
                    DataProcessingAnalytics = await GetDataProcessingAnalyticsAsync(startDate, endDate),
                    BreachAnalytics = await GetBreachAnalyticsAsync(startDate, endDate),
                    ComplianceMetrics = await GetComplianceMetricsAsync()
                };

                switch (format.ToLower())
                {
                    case "json":
                        var jsonData = JsonSerializer.Serialize(analytics, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        return System.Text.Encoding.UTF8.GetBytes(jsonData);

                    case "csv":
                        var csvBuilder = new System.Text.StringBuilder();
                        csvBuilder.AppendLine("Analytics Export Report");
                        csvBuilder.AppendLine($"Generated: {analytics.ExportDate:yyyy-MM-dd HH:mm:ss}");
                        csvBuilder.AppendLine($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                        csvBuilder.AppendLine();
                        
                        // Consent Analytics
                        csvBuilder.AppendLine("Consent Analytics");
                        csvBuilder.AppendLine("Metric,Value");
                        foreach (var item in analytics.ConsentAnalytics)
                        {
                            csvBuilder.AppendLine($"{item.Key},{item.Value}");
                        }
                        csvBuilder.AppendLine();
                        
                        // Data Processing Analytics
                        csvBuilder.AppendLine("Data Processing Analytics");
                        csvBuilder.AppendLine("Metric,Value");
                        foreach (var item in analytics.DataProcessingAnalytics)
                        {
                            csvBuilder.AppendLine($"{item.Key},{item.Value}");
                        }
                        csvBuilder.AppendLine();
                        
                        // Breach Analytics
                        csvBuilder.AppendLine("Breach Analytics");
                        csvBuilder.AppendLine("Metric,Value");
                        foreach (var item in analytics.BreachAnalytics)
                        {
                            csvBuilder.AppendLine($"{item.Key},{item.Value}");
                        }
                        
                        return System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());

                    case "xml":
                        var xmlData = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<AnalyticsExport>
    <ExportDate>{analytics.ExportDate:yyyy-MM-dd HH:mm:ss}</ExportDate>
    <PeriodStart>{startDate:yyyy-MM-dd}</PeriodStart>
    <PeriodEnd>{endDate:yyyy-MM-dd}</PeriodEnd>
    <ConsentAnalytics>
        {string.Join("", analytics.ConsentAnalytics.Select(kv => $"<{kv.Key}>{kv.Value}</{kv.Key}>"))}
    </ConsentAnalytics>
    <DataProcessingAnalytics>
        {string.Join("", analytics.DataProcessingAnalytics.Select(kv => $"<{kv.Key}>{kv.Value}</{kv.Key}>"))}
    </DataProcessingAnalytics>
    <BreachAnalytics>
        {string.Join("", analytics.BreachAnalytics.Select(kv => $"<{kv.Key}>{kv.Value}</{kv.Key}>"))}
    </BreachAnalytics>
</AnalyticsExport>";
                        return System.Text.Encoding.UTF8.GetBytes(xmlData);

                    default:
                        // Default to JSON if format not recognized
                        var defaultJsonData = JsonSerializer.Serialize(analytics, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        return System.Text.Encoding.UTF8.GetBytes(defaultJsonData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting analytics data for period {StartDate} to {EndDate} in format {Format}",
                    startDate, endDate, format);
                
                // Return error information as JSON
                var errorData = JsonSerializer.Serialize(new
                {
                    error = "Export failed",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                });
                return System.Text.Encoding.UTF8.GetBytes(errorData);
            }
        }

        public async Task<Dictionary<string, object>> GetSystemStatusAsync()
        {
            try
            {
                var systemStatus = new Dictionary<string, object>();

                // Check database connectivity
                var dbStatus = await CheckDatabaseStatusAsync();
                systemStatus["dataProcessing"] = dbStatus ? "Operational" : "Error";

                // Check consent management (recent consent records)
                var recentConsents = await _context.ConsentRecords
                    .Where(c => c.ConsentDate >= DateTime.UtcNow.AddHours(-24))
                    .CountAsync();
                systemStatus["consentManagement"] = recentConsents >= 0 ? "Operational" : "Warning";

                // Check data retention (active policies)
                var activePolicies = await _context.DataRetentionPolicies
                    .CountAsync(p => p.IsActive);
                systemStatus["dataRetention"] = activePolicies > 0 ? "Operational" : "Warning";

                // Check security monitoring (recent audit logs)
                var recentAudits = await _context.GdprAuditLogs
                    .Where(l => l.Timestamp >= DateTime.UtcNow.AddHours(-24))
                    .CountAsync();
                systemStatus["securityMonitoring"] = recentAudits >= 0 ? "Operational" : "Warning";

                // Check for any active breach incidents
                var activeBreaches = await _context.BreachIncidents
                    .CountAsync(b => b.Status != "Resolved");
                if (activeBreaches > 0)
                {
                    systemStatus["securityMonitoring"] = "Critical";
                }

                // Overall system health
                var criticalCount = systemStatus.Values.Count(v => v.ToString() == "Critical");
                var errorCount = systemStatus.Values.Count(v => v.ToString() == "Error");
                var warningCount = systemStatus.Values.Count(v => v.ToString() == "Warning");

                string overallStatus = "Operational";
                if (criticalCount > 0) overallStatus = "Critical";
                else if (errorCount > 0) overallStatus = "Error";
                else if (warningCount > 0) overallStatus = "Warning";

                systemStatus["overall"] = overallStatus;
                systemStatus["lastUpdated"] = DateTime.UtcNow;

                return systemStatus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system status");
                return new Dictionary<string, object>
                {
                    ["dataProcessing"] = "Error",
                    ["consentManagement"] = "Error",
                    ["dataRetention"] = "Error",
                    ["securityMonitoring"] = "Error",
                    ["overall"] = "Error",
                    ["lastUpdated"] = DateTime.UtcNow
                };
            }
        }

        private async Task<bool> CheckDatabaseStatusAsync()
        {
            try
            {
                // Simple database connectivity check
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connectivity check failed");
                return false;
            }
        }
    }
}