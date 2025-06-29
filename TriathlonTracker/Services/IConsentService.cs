using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public interface IConsentService
    {
        // Consent Management
        Task<bool> GrantConsentAsync(string userId, string consentType, string purpose, string legalBasis, string consentVersion = "1.0");
        Task<bool> WithdrawConsentAsync(string userId, string consentType);
        Task<bool> UpdateConsentAsync(string userId, string consentType, bool isGranted, string? reason = null);
        
        // Consent Verification
        Task<bool> HasValidConsentAsync(string userId, string consentType);
        Task<ConsentRecord?> GetLatestConsentAsync(string userId, string consentType);
        Task<IEnumerable<ConsentRecord>> GetConsentHistoryAsync(string userId);
        
        // Consent Types
        Task<bool> HasDataProcessingConsentAsync(string userId);
        Task<bool> HasMarketingConsentAsync(string userId);
        Task<bool> HasAnalyticsConsentAsync(string userId);
        
        // Consent Validation
        Task<bool> IsConsentRequiredAsync(string consentType);
        Task<bool> IsConsentValidAsync(string userId, string consentType);
        Task<DateTime?> GetConsentExpirationDateAsync(string userId, string consentType);
        
        // Consent Reporting
        Task<object> GetConsentSummaryAsync(string userId);
        Task<IEnumerable<ConsentRecord>> GetExpiredConsentsAsync();
        Task<int> GetConsentRateAsync(string consentType);
        
        // Consent Renewal
        Task<bool> IsConsentRenewalRequiredAsync(string userId, string consentType);
        Task<bool> RenewConsentAsync(string userId, string consentType, string newVersion);
        
        // Bulk Operations
        Task<bool> BulkWithdrawConsentAsync(string userId, IEnumerable<string> consentTypes);
        Task<bool> MigrateConsentToNewVersionAsync(string oldVersion, string newVersion);
    }
}