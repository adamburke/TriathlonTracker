using Microsoft.AspNetCore.Identity;

namespace TriathlonTracker.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // GDPR-related fields
        public DateTime? ConsentDate { get; set; }
        public string ConsentVersion { get; set; } = "1.0";
        public bool HasGivenConsent { get; set; } = false;
        public DateTime? ConsentWithdrawnDate { get; set; }
        public DateTime? LastDataExportDate { get; set; }
        public DateTime? AccountDeletionRequestDate { get; set; }
        public bool IsAccountDeletionRequested { get; set; } = false;
        public string DataRetentionPreference { get; set; } = "Standard"; // "Standard", "Minimal", "Extended"
        public bool MarketingConsent { get; set; } = false;
        public bool AnalyticsConsent { get; set; } = false;
        public string PreferredDataFormat { get; set; } = "JSON"; // For data exports
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public ICollection<ConsentRecord> ConsentRecords { get; set; } = new List<ConsentRecord>();
        public ICollection<DataProcessingLog> DataProcessingLogs { get; set; } = new List<DataProcessingLog>();
    }
}