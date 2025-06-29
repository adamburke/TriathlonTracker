using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;
using TriathlonTracker.Models.Enums;

namespace TriathlonTracker.Services
{
    public class GdprMonitoringService : IGdprMonitoringService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GdprMonitoringService> _logger;

        public GdprMonitoringService(ApplicationDbContext context, ILogger<GdprMonitoringService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<SecurityStatus> GetSecurityStatusAsync()
        {
            var status = new SecurityStatus
            {
                Id = Guid.NewGuid().ToString(),
                OverallStatus = "Good",
                LastSecurityScan = DateTime.UtcNow.AddHours(-2),
                ActiveThreats = 0,
                SecurityScore = 85,
                RecentIncidents = 0,
                SystemHealth = "Operational",
                LastUpdated = DateTime.UtcNow
            };

            return Task.FromResult(status);
        }

        public async Task<IEnumerable<SecurityEvent>> GetRecentSecurityEventsAsync(int count)
        {
            return await _context.SecurityEvents
                .OrderByDescending(e => e.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<IpAccessControl>> GetIpAccessControlsAsync()
        {
            return await _context.IpAccessControls
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task LogSecurityEventAsync(string eventType, string description, string? userId = null, string? ipAddress = null)
        {
            try
            {
                var securityEvent = new SecurityEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = eventType,
                    Description = description,
                    UserId = userId,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    Severity = DetermineSeverity(eventType),
                    Source = "System"
                };

                _context.SecurityEvents.Add(securityEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging security event: {EventType}", eventType);
            }
        }

        public Task<ComplianceMetrics> GetComplianceMetricsAsync()
        {
            var metrics = new ComplianceMetrics
            {
                Id = Guid.NewGuid().ToString(),
                TotalUsers = 0,
                ConsentedUsers = 0,
                PendingRequests = 0,
                ComplianceScore = 95.5,
                DataProcessingActivities = 0,
                RetentionPoliciesActive = 0,
                LastCalculated = DateTime.UtcNow
            };

            return Task.FromResult(metrics);
        }

        public async Task<IEnumerable<AuditLogEntry>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AuditLogEntries.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Take(1000)
                .ToListAsync();
        }

        public async Task CreateAuditLogAsync(string action, string entityType, string? entityId, string details, string? userId = null)
        {
            try
            {
                var auditLog = new AuditLogEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Details = details,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = null // Could be populated from HttpContext if needed
                };

                _context.AuditLogEntries.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log: {Action}", action);
            }
        }

        private string DetermineSeverity(string eventType)
        {
            return eventType.ToLower() switch
            {
                "login_failed" => "Warning",
                "unauthorized_access" => "Error",
                "data_breach" => "Critical",
                "suspicious_activity" => "Error",
                _ => "Information"
            };
        }
    }
}