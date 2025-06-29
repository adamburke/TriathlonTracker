using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public interface IGdprMonitoringService
    {
        Task<SecurityStatus> GetSecurityStatusAsync();
        Task<IEnumerable<SecurityEvent>> GetRecentSecurityEventsAsync(int count);
        Task<IEnumerable<IpAccessControl>> GetIpAccessControlsAsync();
        Task LogSecurityEventAsync(string eventType, string description, string? userId = null, string? ipAddress = null);
        Task<ComplianceMetrics> GetComplianceMetricsAsync();
        Task<IEnumerable<AuditLogEntry>> GetAuditLogsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task CreateAuditLogAsync(string action, string entityType, string? entityId, string details, string? userId = null);
    }
}