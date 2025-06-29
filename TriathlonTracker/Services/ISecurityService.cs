using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public interface ISecurityService
    {
        Task<SecurityStatus> GetSecurityStatusAsync();
        Task<IEnumerable<SecurityEvent>> GetRecentSecurityEventsAsync(int count);
        Task<IEnumerable<IpAccessControl>> GetIpAccessControlsAsync();
        Task<bool> AddIpAccessControlAsync(string ipAddress, string accessType, string reason);
        Task<bool> RemoveIpAccessControlAsync(string controlId);
        Task LogSecurityEventAsync(string eventType, string description, string? userId = null, string? ipAddress = null);
        Task<bool> IsIpAllowedAsync(string ipAddress);
        Task<SecurityMetrics> GetSecurityMetricsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SecurityEvent>> GetSecurityEventsAsync(DateTime? startDate = null, DateTime? endDate = null, string? eventType = null);
    }
}