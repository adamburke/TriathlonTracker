using TriathlonTracker.Models;

namespace TriathlonTracker.Services.Interfaces
{
    public interface IComplianceService
    {
        Task<GdprComplianceMetrics> GetComplianceMetricsAsync();
        Task<bool> RunComplianceCheckAsync();
        Task<bool> CreateComplianceAlertAsync(string type, string title, string description, string severity);
        Task<bool> ResolveAlertAsync(string alertId, string resolutionNotes);
        Task<Dictionary<string, object>> GetComplianceStatusAsync();
        Task<ComplianceReport> GenerateComplianceReportAsync(string reportType, DateTime startDate, DateTime endDate);
        Task<byte[]> ExportComplianceReportAsync(string reportId, string format);
        Task<List<ComplianceReport>> GetReportsAsync(int page = 1, int pageSize = 20);
        Task<Dictionary<string, object>> GetConsentAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, object>> GetDataProcessingAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, object>> GetBreachAnalyticsAsync(DateTime startDate, DateTime endDate);
    }
} 