using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TriathlonTracker.Models.Enums;

namespace TriathlonTracker.Models
{
    public class User : IdentityUser
    {
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // GDPR-related fields
        public DateTime? ConsentDate { get; set; }
        
        [StringLength(20)]
        public string ConsentVersion { get; set; } = "1.0";
        
        public bool HasGivenConsent { get; set; } = false;
        
        public DateTime? ConsentWithdrawnDate { get; set; }
        
        public DateTime? LastDataExportDate { get; set; }
        
        public DateTime? AccountDeletionRequestDate { get; set; }
        
        public bool IsAccountDeletionRequested { get; set; } = false;
        
        public DataRetentionPreference DataRetentionPreference { get; set; } = DataRetentionPreference.Standard;
        
        public bool MarketingConsent { get; set; } = false;
        
        public bool AnalyticsConsent { get; set; } = false;
        
        [StringLength(10)]
        public string PreferredDataFormat { get; set; } = "JSON"; // For data exports
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public ICollection<ConsentRecord> ConsentRecords { get; set; } = new List<ConsentRecord>();
        public ICollection<DataProcessingLog> DataProcessingLogs { get; set; } = new List<DataProcessingLog>();
    }
}