using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public interface IAdminDashboardService
    {
        // Dashboard Data
        Task<AdminDashboardViewModel> GetDashboardDataAsync();
        Task<GdprComplianceMetrics> GetComplianceMetricsAsync();
        Task<List<UserGdprStatus>> GetUserGdprStatusesAsync(int page = 1, int pageSize = 50);
        Task<List<RecentActivity>> GetRecentActivitiesAsync(int count = 20);
        Task<List<ComplianceAlert>> GetActiveAlertsAsync();
        Task<DataRetentionSummary> GetRetentionSummaryAsync();
        
        // User Management
        Task<UserGdprStatus?> GetUserGdprStatusAsync(string userId);
        Task<bool> UpdateUserDetailsAsync(string userId, string firstName, string lastName, string email, bool hasConsent);
        Task<bool> UpdateUserConsentAsync(string userId, string consentType, bool isGranted, string reason);
        Task<bool> BulkUpdateConsentAsync(BulkConsentOperation operation);
        Task<List<UserGdprStatus>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 50);
        Task<bool> DeleteUserAsync(string userId);
        
        // Compliance Monitoring
        Task<bool> RunComplianceCheckAsync();
        Task<bool> CreateComplianceAlertAsync(string type, string title, string description, string severity);
        Task<bool> ResolveAlertAsync(string alertId, string resolutionNotes);
        Task<Dictionary<string, object>> GetComplianceStatusAsync();
        
        // Data Retention Management
        Task<bool> ExecuteRetentionPolicyAsync(string policyId);
        Task<List<RetentionPolicyStatus>> GetRetentionPolicyStatusesAsync();
        Task<bool> ScheduleDataCleanupAsync(DateTime scheduledDate);
        Task<List<string>> GetExpiredDataSummaryAsync();
        
        // Reporting
        Task<ComplianceReport> GenerateComplianceReportAsync(string reportType, DateTime startDate, DateTime endDate);
        Task<byte[]> ExportComplianceReportAsync(string reportId, string format);
        Task<List<ComplianceReport>> GetReportsAsync(int page = 1, int pageSize = 20);
        
        // Analytics
        Task<Dictionary<string, object>> GetConsentAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, object>> GetDataProcessingAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, object>> GetBreachAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportAnalyticsDataAsync(DateTime startDate, DateTime endDate, string format);
        
        // System Status
        Task<Dictionary<string, object>> GetSystemStatusAsync();
        
        // Audit Trail
        Task LogAdminActionAsync(string action, string entityType, string entityId, string details);
        Task<List<GdprAuditLog>> GetAuditLogsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50);
        Task<List<GdprAuditLog>> SearchAuditLogsAsync(string searchTerm, int page = 1, int pageSize = 50);
    }
}