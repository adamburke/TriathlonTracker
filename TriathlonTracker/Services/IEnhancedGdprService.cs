using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public interface IEnhancedGdprService
    {
        // Enhanced Data Export
        Task<DataExportRequest> CreateDataExportRequestAsync(string userId, string format);
        Task<bool> ProcessDataExportRequestAsync(string requestId);
        Task<string> GenerateSecureDownloadLinkAsync(string requestId);
        Task<IEnumerable<DataExportRequest>> GetUserExportRequestsAsync(string userId);
        Task<bool> CleanupExpiredExportRequestsAsync();
        Task<byte[]> ExportUserDataInFormatAsync(string userId, string format);
        
        // Data Rectification System
        Task<DataRectificationRequest> CreateRectificationRequestAsync(string userId, string dataType, string fieldName, string currentValue, string requestedValue, string reason);
        Task<bool> ReviewRectificationRequestAsync(string requestId, bool approved, string reviewNotes, string reviewedBy);
        Task<bool> ProcessApprovedRectificationAsync(string requestId);
        Task<IEnumerable<DataRectificationRequest>> GetUserRectificationRequestsAsync(string userId);
        Task<IEnumerable<DataRectificationRequest>> GetPendingRectificationRequestsAsync();
        Task<bool> ValidateRectificationRequestAsync(DataRectificationRequest request);
        
        // Enhanced Account Deletion
        Task<AccountDeletionRequest> CreateAccountDeletionRequestAsync(string userId, string reason, string deletionType = "SoftDelete");
        Task<bool> ConfirmAccountDeletionAsync(string confirmationToken);
        Task<bool> ProcessAccountDeletionAsync(string requestId);
        Task<bool> RecoverAccountAsync(string userId);
        Task<IEnumerable<AccountDeletionRequest>> GetPendingDeletionRequestsAsync();
        Task<bool> CleanupExpiredDeletionRequestsAsync();
        
        // Data Portability Enhancements
        Task<bool> ImportUserDataAsync(string userId, string jsonData);
        Task<bool> ValidateImportDataAsync(string jsonData);
        Task<object> GetDataPortabilityStatusAsync(string userId);
        Task<bool> CreateDataMigrationJobAsync(string fromUserId, string toUserId);
        
        // Advanced Consent Management
        Task<bool> CreateConsentVersionAsync(string version, string description, DateTime effectiveDate);
        Task<IEnumerable<ConsentRecord>> GetConsentHistoryWithVersionsAsync(string userId);
        Task<bool> NotifyConsentRenewalRequiredAsync(string userId, string consentType);
        Task<object> GetConsentAnalyticsAsync(DateTime fromDate, DateTime toDate);
        Task<bool> BulkUpdateConsentVersionAsync(string oldVersion, string newVersion);
        
        // Breach Detection and Response
        Task<BreachIncident> CreateBreachIncidentAsync(string breachType, string severity, string description, string[] affectedDataTypes, string[] affectedUserIds);
        Task<bool> UpdateBreachIncidentAsync(int incidentId, string status, string notes, string updatedBy);
        Task<bool> NotifyUsersOfBreachAsync(int incidentId);
        Task<bool> NotifyRegulatorsOfBreachAsync(int incidentId);
        Task<IEnumerable<BreachIncident>> GetActiveBreachIncidentsAsync();
        Task<object> GetBreachStatisticsAsync(DateTime fromDate, DateTime toDate);
        Task<bool> DetectPotentialBreachAsync(string userId, string action, string ipAddress);
        
        // Enhanced Privacy Dashboard
        Task<object> GetEnhancedPrivacyDashboardAsync(string userId);
        Task<object> GetDataUsageAnalyticsAsync(string userId, DateTime fromDate, DateTime toDate);
        Task<object> GetDataRetentionTimelineAsync(string userId);
        Task<IEnumerable<object>> GetPrivacySettingsAsync(string userId);
        Task<bool> UpdatePrivacySettingsAsync(string userId, Dictionary<string, object> settings);
        
        // API Support
        Task<bool> ValidateApiRequestAsync(string apiKey, string endpoint);
        Task<bool> LogApiAccessAsync(string apiKey, string endpoint, string userId, bool success);
        Task<object> GetApiUsageStatisticsAsync(string apiKey, DateTime fromDate, DateTime toDate);
        Task<bool> RateLimitCheckAsync(string apiKey, string endpoint);
        
        // Notification System
        Task<bool> SendGdprNotificationAsync(string userId, string notificationType, Dictionary<string, object> parameters);
        Task<bool> ScheduleGdprNotificationAsync(string userId, string notificationType, DateTime scheduledDate, Dictionary<string, object> parameters);
        Task<IEnumerable<object>> GetPendingNotificationsAsync();
        Task<bool> ProcessPendingNotificationsAsync();
        
        // Compliance Reporting
        Task<object> GenerateEnhancedComplianceReportAsync(DateTime fromDate, DateTime toDate);
        Task<object> GenerateDataSubjectRequestReportAsync(DateTime fromDate, DateTime toDate);
        Task<object> GenerateBreachReportAsync(DateTime fromDate, DateTime toDate);
        Task<object> GenerateConsentReportAsync(DateTime fromDate, DateTime toDate);
        
        // Data Quality and Validation
        Task<bool> ValidateUserDataIntegrityAsync(string userId);
        Task<object> GetDataQualityReportAsync(string userId);
        Task<bool> FixDataQualityIssuesAsync(string userId, string[] issueTypes);
        
        // Audit and Monitoring
        Task<bool> CreateAuditLogAsync(string action, string userId, string details, string performedBy);
        Task<IEnumerable<object>> GetAuditLogsAsync(DateTime fromDate, DateTime toDate, string? userId = null);
        Task<bool> MonitorGdprComplianceAsync();
        Task<object> GetComplianceMetricsAsync();
    }
}