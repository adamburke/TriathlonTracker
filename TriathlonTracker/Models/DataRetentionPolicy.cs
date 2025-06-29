using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class DataRetentionPolicy
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string DataType { get; set; } = string.Empty; // e.g., "PersonalData", "TriathlonData", "ConsentRecords"
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public int RetentionPeriodDays { get; set; } // Number of days to retain data
        
        [Required]
        [StringLength(100)]
        public string LegalBasis { get; set; } = string.Empty; // Legal basis for retention
        
        [StringLength(500)]
        public string RetentionReason { get; set; } = string.Empty; // Reason for retention period
        
        [Required]
        public bool IsActive { get; set; } = true;
        
        [Required]
        public bool AutoDelete { get; set; } = false; // Whether to automatically delete expired data
        
        [StringLength(100)]
        public string DeletionMethod { get; set; } = "SoftDelete"; // "SoftDelete", "HardDelete", "Anonymize"
        
        [StringLength(500)]
        public string NotificationSettings { get; set; } = string.Empty; // JSON for notification preferences
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
    }
}