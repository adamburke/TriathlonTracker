using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SecurityService> _logger;

        public SecurityService(ApplicationDbContext context, ILogger<SecurityService> logger)
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

        public async Task<bool> AddIpAccessControlAsync(string ipAddress, string accessType, string reason)
        {
            try
            {
                var control = new IpAccessControl
                {
                    Id = Guid.NewGuid().ToString(),
                    IpAddress = ipAddress,
                    AccessType = accessType,
                    Reason = reason,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                };

                _context.IpAccessControls.Add(control);
                await _context.SaveChangesAsync();

                await LogSecurityEventAsync("IP_ACCESS_CONTROL_ADDED", $"IP access control added for {ipAddress}: {accessType}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding IP access control for {IpAddress}", ipAddress);
                return false;
            }
        }

        public async Task<bool> RemoveIpAccessControlAsync(string controlId)
        {
            try
            {
                var control = await _context.IpAccessControls.FindAsync(controlId);
                if (control == null) return false;

                control.IsActive = false;
                // Note: RemovedAt property doesn't exist in IpAccessControl model
                // Using metadata to track removal time
                control.Metadata["RemovedAt"] = DateTime.UtcNow.ToString("O");

                await _context.SaveChangesAsync();

                await LogSecurityEventAsync("IP_ACCESS_CONTROL_REMOVED", $"IP access control removed for {control.IpAddress}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing IP access control {ControlId}", controlId);
                return false;
            }
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
                    Source = "SecurityService"
                };

                _context.SecurityEvents.Add(securityEvent);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging security event: {EventType}", eventType);
            }
        }

        public async Task<bool> IsIpAllowedAsync(string ipAddress)
        {
            try
            {
                var blockedControl = await _context.IpAccessControls
                    .Where(c => c.IsActive && c.IpAddress == ipAddress && c.AccessType == "Block")
                    .FirstOrDefaultAsync();

                return blockedControl == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking IP access for {IpAddress}", ipAddress);
                return true; // Default to allow on error
            }
        }

        public Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime startDate, DateTime endDate)
        {
            var metrics = new SecurityMetrics
            {
                Id = Guid.NewGuid().ToString(),
                TotalSecurityEvents = 0,
                HighSeverityEvents = 0,
                BlockedIpAddresses = 0,
                FailedLoginAttempts = 0,
                SuspiciousActivities = 0,
                SecurityScore = 85,
                StartDate = startDate,
                EndDate = endDate,
                GeneratedAt = DateTime.UtcNow
            };

            return Task.FromResult(metrics);
        }

        public async Task<IEnumerable<SecurityEvent>> GetSecurityEventsAsync(DateTime? startDate = null, DateTime? endDate = null, string? eventType = null)
        {
            var query = _context.SecurityEvents.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.Timestamp <= endDate.Value);

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(e => e.EventType == eventType);

            return await query
                .OrderByDescending(e => e.Timestamp)
                .Take(1000)
                .ToListAsync();
        }

        private string DetermineSeverity(string eventType)
        {
            return eventType.ToLower() switch
            {
                "login_failed" => "Medium",
                "unauthorized_access" => "High",
                "data_breach" => "Critical",
                "suspicious_activity" => "High",
                "ip_access_control_added" => "Low",
                "ip_access_control_removed" => "Low",
                _ => "Low"
            };
        }
    }
}