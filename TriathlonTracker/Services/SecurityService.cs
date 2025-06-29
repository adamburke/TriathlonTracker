using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Services;

namespace TriathlonTracker.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SecurityService> _logger;
        private readonly IAuditService _auditService;

        public SecurityService(ApplicationDbContext context, ILogger<SecurityService> logger, IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
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
                _logger.LogDebug("Logging security event: {EventType} - {Description} for user {UserId}", eventType, description, userId ?? "System");
                
                var securityEvent = new SecurityEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = eventType,
                    Description = description,
                    UserId = userId ?? "System",
                    IpAddress = ipAddress ?? "System",
                    Timestamp = DateTime.UtcNow,
                    Severity = DetermineSeverity(eventType)
                };

                _context.SecurityEvents.Add(securityEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Security event logged: {EventType} for user {UserId}", eventType, userId ?? "System");
                await _auditService.LogAsync("LogSecurityEvent", "Security", userId ?? "System", $"Security event: {eventType} - {description}", userId ?? "System", ipAddress ?? "System", "System", "Information");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging security event: {EventType} for user {UserId}", eventType, userId ?? "System");
                await _auditService.LogAsync("LogSecurityEventError", "Security", userId ?? "System", $"Error logging security event: {ex.Message}", userId ?? "System", ipAddress ?? "System", "System", "Error");
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
            try
            {
                _logger.LogDebug("Retrieving security events with filters: startDate={StartDate}, endDate={EndDate}, eventType={EventType}", startDate, endDate, eventType);
                
                var query = _context.SecurityEvents.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(e => e.Timestamp >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(e => e.Timestamp <= endDate.Value);
                if (!string.IsNullOrEmpty(eventType))
                    query = query.Where(e => e.EventType == eventType);

                var events = await query.OrderByDescending(e => e.Timestamp).ToListAsync();

                _logger.LogInformation("Retrieved {Count} security events", events.Count);
                await _auditService.LogAsync("GetSecurityEvents", "Security", null, $"Retrieved {events.Count} security events", "System", "System", "System", "Information");
                
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security events");
                await _auditService.LogAsync("GetSecurityEventsError", "Security", null, $"Error: {ex.Message}", "System", "System", "System", "Error");
                return Enumerable.Empty<SecurityEvent>();
            }
        }

        public async Task<bool> CheckForSuspiciousActivityAsync(string userId, string ipAddress, string userAgent)
        {
            try
            {
                _logger.LogDebug("Checking for suspicious activity for user {UserId} from IP {IpAddress}", userId, ipAddress);
                
                // Check for multiple failed login attempts
                var recentFailedLogins = await _context.SecurityEvents
                    .Where(e => e.UserId == userId && e.EventType == "LOGIN_FAILED" && e.Timestamp >= DateTime.UtcNow.AddMinutes(15))
                    .CountAsync();

                if (recentFailedLogins >= 5)
                {
                    _logger.LogWarning("Suspicious activity detected for user {UserId}: {Count} failed login attempts in 15 minutes", userId, recentFailedLogins);
                    await LogSecurityEventAsync("SUSPICIOUS_ACTIVITY", $"Multiple failed login attempts: {recentFailedLogins} in 15 minutes", userId, ipAddress);
                    await _auditService.LogAsync("SuspiciousActivityDetected", "Security", userId, $"Multiple failed login attempts: {recentFailedLogins}", userId, ipAddress, userAgent, "Warning");
                    return true;
                }

                // Check for unusual access patterns
                var recentLogins = await _context.SecurityEvents
                    .Where(e => e.UserId == userId && e.EventType == "LOGIN_SUCCESS" && e.Timestamp >= DateTime.UtcNow.AddHours(1))
                    .CountAsync();

                if (recentLogins > 10)
                {
                    _logger.LogWarning("Suspicious activity detected for user {UserId}: {Count} successful logins in 1 hour", userId, recentLogins);
                    await LogSecurityEventAsync("SUSPICIOUS_ACTIVITY", $"Unusual login frequency: {recentLogins} in 1 hour", userId, ipAddress);
                    await _auditService.LogAsync("SuspiciousActivityDetected", "Security", userId, $"Unusual login frequency: {recentLogins}", userId, ipAddress, userAgent, "Warning");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for suspicious activity for user {UserId}", userId);
                await _auditService.LogAsync("CheckSuspiciousActivityError", "Security", userId, $"Error: {ex.Message}", userId, ipAddress, userAgent, "Error");
                return false;
            }
        }

        public async Task<bool> LogLoginAttemptAsync(string userId, bool success, string ipAddress, string userAgent, string? failureReason = null)
        {
            try
            {
                var eventType = success ? "LOGIN_SUCCESS" : "LOGIN_FAILED";
                var description = success ? "Successful login" : $"Failed login: {failureReason}";
                
                _logger.LogDebug("Logging login attempt for user {UserId}: {Success} from IP {IpAddress}", userId, success, ipAddress);
                
                await LogSecurityEventAsync(eventType, description, userId, ipAddress);
                await _auditService.LogAsync("LogLoginAttempt", "Security", userId, $"Login attempt: {(success ? "Success" : "Failed")} - {failureReason}", userId, ipAddress, userAgent, success ? "Information" : "Warning");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging login attempt for user {UserId}", userId);
                await _auditService.LogAsync("LogLoginAttemptError", "Security", userId, $"Error: {ex.Message}", userId, ipAddress, userAgent, "Error");
                return false;
            }
        }

        public async Task<bool> LogLogoutAsync(string userId, string ipAddress, string userAgent)
        {
            try
            {
                _logger.LogDebug("Logging logout for user {UserId} from IP {IpAddress}", userId, ipAddress);
                
                await LogSecurityEventAsync("LOGOUT", "User logged out", userId, ipAddress);
                await _auditService.LogAsync("LogLogout", "Security", userId, "User logged out", userId, ipAddress, userAgent, "Information");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging logout for user {UserId}", userId);
                await _auditService.LogAsync("LogLogoutError", "Security", userId, $"Error: {ex.Message}", userId, ipAddress, userAgent, "Error");
                return false;
            }
        }

        public async Task<bool> LogDataAccessAsync(string userId, string dataType, string operation, string ipAddress, string userAgent)
        {
            try
            {
                _logger.LogDebug("Logging data access for user {UserId}: {Operation} on {DataType} from IP {IpAddress}", userId, operation, dataType, ipAddress);
                
                await LogSecurityEventAsync("DATA_ACCESS", $"{operation} on {dataType}", userId, ipAddress);
                await _auditService.LogAsync("LogDataAccess", "Security", userId, $"{operation} on {dataType}", userId, ipAddress, userAgent, "Information");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging data access for user {UserId}", userId);
                await _auditService.LogAsync("LogDataAccessError", "Security", userId, $"Error: {ex.Message}", userId, ipAddress, userAgent, "Error");
                return false;
            }
        }

        public async Task<IEnumerable<SecurityEvent>> GetSecurityReportAsync(DateTime startDate, DateTime endDate, string? userId = null)
        {
            try
            {
                _logger.LogDebug("Generating security report from {StartDate} to {EndDate} for user {UserId}", startDate, endDate, userId ?? "All");
                
                var query = _context.SecurityEvents.AsQueryable();

                query = query.Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate);

                if (!string.IsNullOrEmpty(userId))
                    query = query.Where(e => e.UserId == userId);

                var events = await query.OrderByDescending(e => e.Timestamp).ToListAsync();

                _logger.LogInformation("Generated security report with {Count} events", events.Count);
                await _auditService.LogAsync("GetSecurityReport", "Security", userId, $"Generated security report with {events.Count} events", userId ?? "System", "System", "System", "Information");
                
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating security report");
                await _auditService.LogAsync("GetSecurityReportError", "Security", userId, $"Error: {ex.Message}", userId ?? "System", "System", "System", "Error");
                return Enumerable.Empty<SecurityEvent>();
            }
        }

        private string DetermineSeverity(string eventType)
        {
            return eventType switch
            {
                "LOGIN_FAILED" => "Warning",
                "SUSPICIOUS_ACTIVITY" => "Warning",
                "SECURITY_BREACH" => "Error",
                "DATA_ACCESS" => "Information",
                "LOGIN_SUCCESS" => "Information",
                "LOGOUT" => "Information",
                _ => "Information"
            };
        }
    }
}