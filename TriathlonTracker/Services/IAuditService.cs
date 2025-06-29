using System.Threading.Tasks;
using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public interface IAuditService
    {
        Task LogAsync(AuditLog log);
        Task LogAsync(string action, string entityType, string? entityId, string? details, string? userId, string? ipAddress, string? userAgent, string logLevel = "Information");
    }
} 