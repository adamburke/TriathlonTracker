using TriathlonTracker.Models;

namespace TriathlonTracker.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAdminActionAsync(string action, string entityType, string entityId, string details);
        Task<List<GdprAuditLog>> GetAuditLogsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50);
        Task<List<GdprAuditLog>> SearchAuditLogsAsync(string searchTerm, int page = 1, int pageSize = 50);
    }
} 