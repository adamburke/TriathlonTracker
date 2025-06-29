using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class DataRectificationRequest
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string DataType { get; set; } = string.Empty; // PersonalData, TriathlonData, etc.
        
        [Required]
        [StringLength(100)]
        public string FieldName { get; set; } = string.Empty; // Field to be corrected
        
        [StringLength(500)]
        public string CurrentValue { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string RequestedValue { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty; // Reason for correction
        
        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, UnderReview, Approved, Rejected, Completed
        
        public DateTime? ReviewDate { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        [StringLength(100)]
        public string ReviewedBy { get; set; } = string.Empty; // Admin who reviewed
        
        [StringLength(1000)]
        public string ReviewNotes { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string RejectionReason { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        public bool IsNotificationSent { get; set; } = false;
        
        [StringLength(1000)]
        public string SupportingDocuments { get; set; } = string.Empty; // JSON array of document paths
        
        public int Priority { get; set; } = 1; // 1=Low, 2=Medium, 3=High
        
        // Navigation Properties
        public User? User { get; set; }
    }
}