using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public interface IGdprService
    {
        // Data Export
        Task<string> ExportUserDataAsync(string userId, string format = "JSON");
        Task<byte[]> ExportUserDataAsPdfAsync(string userId);
        
        // Data Deletion
        Task<bool> RequestAccountDeletionAsync(string userId, string reason = "");
        Task<bool> ProcessAccountDeletionAsync(string userId);
        Task<bool> AnonymizeUserDataAsync(string userId);
        
        // Data Processing Logging
        Task LogDataProcessingAsync(string userId, string action, string dataType, string purpose, string legalBasis, string? additionalData = null);
        Task<IEnumerable<DataProcessingLog>> GetDataProcessingLogsAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
        
        // Data Retention
        Task<bool> ApplyDataRetentionPoliciesAsync();
        Task<IEnumerable<DataRetentionPolicy>> GetDataRetentionPoliciesAsync();
        Task<bool> IsDataExpiredAsync(string dataType, DateTime createdDate);
        
        // User Rights
        Task<bool> HasUserGivenConsentAsync(string userId, string consentType);
        Task<DateTime?> GetLastDataExportDateAsync(string userId);
        Task<bool> IsAccountDeletionRequestedAsync(string userId);
        
        // Compliance Reporting
        Task<object> GenerateComplianceReportAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<string>> GetExpiredDataTypesAsync();
        
        // Data Portability
        Task<bool> ValidateDataExportRequestAsync(string userId);
        Task<string> GetUserDataSummaryAsync(string userId);
    }
}