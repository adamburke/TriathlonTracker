using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class ConsentRecord
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ConsentType { get; set; } = string.Empty; // e.g., "DataProcessing", "Marketing", "Analytics"
        
        [Required]
        public bool IsGranted { get; set; }
        
        [Required]
        public DateTime ConsentDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? WithdrawnDate { get; set; }
        
        [StringLength(500)]
        public string Purpose { get; set; } = string.Empty; // Purpose of data processing
        
        [StringLength(100)]
        public string LegalBasis { get; set; } = string.Empty; // GDPR legal basis (e.g., "Consent", "Contract", "LegitimateInterest")
        
        [StringLength(50)]
        public string ConsentVersion { get; set; } = "1.0"; // Version of consent terms
        
        [StringLength(200)]
        public string IpAddress { get; set; } = string.Empty; // IP address when consent was given
        
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty; // Browser user agent
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public User? User { get; set; }
    }
}