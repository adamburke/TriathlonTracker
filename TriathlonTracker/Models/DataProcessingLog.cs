using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class DataProcessingLog
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // e.g., "DataAccessed", "DataModified", "DataExported", "DataDeleted"
        
        [Required]
        [StringLength(100)]
        public string DataType { get; set; } = string.Empty; // e.g., "PersonalData", "TriathlonData", "AccountData"
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty; // Detailed description of the action
        
        [Required]
        [StringLength(100)]
        public string Purpose { get; set; } = string.Empty; // Purpose of processing
        
        [Required]
        [StringLength(100)]
        public string LegalBasis { get; set; } = string.Empty; // GDPR legal basis
        
        [StringLength(200)]
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string ProcessedBy { get; set; } = string.Empty; // System or user who performed the action
        
        public bool IsAutomated { get; set; } = false; // Whether the processing was automated
        
        [StringLength(1000)]
        public string AdditionalData { get; set; } = string.Empty; // JSON string for additional context
        
        [Required]
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public User? User { get; set; }
    }
}