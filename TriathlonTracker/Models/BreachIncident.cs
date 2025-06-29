using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class BreachIncident
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string IncidentId { get; set; } = string.Empty; // Unique incident identifier
        
        [Required]
        [StringLength(100)]
        public string BreachType { get; set; } = string.Empty; // DataBreach, SecurityIncident, UnauthorizedAccess
        
        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        [Required]
        public DateTime DetectedDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? OccurredDate { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string AffectedDataTypes { get; set; } = string.Empty; // JSON array of data types
        
        public int EstimatedAffectedUsers { get; set; } = 0;
        
        [StringLength(1000)]
        public string AffectedUserIds { get; set; } = string.Empty; // JSON array of user IDs
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Open"; // Open, UnderInvestigation, Contained, Resolved, Closed
        
        [StringLength(100)]
        public string DetectedBy { get; set; } = "System"; // System, User, Admin, External
        
        [StringLength(100)]
        public string AssignedTo { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string ImpactAssessment { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string ContainmentActions { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string RemediationActions { get; set; } = string.Empty;
        
        public bool RequiresRegulatoryNotification { get; set; } = false;
        
        public DateTime? RegulatoryNotificationDate { get; set; }
        
        public bool RequiresUserNotification { get; set; } = false;
        
        public DateTime? UserNotificationDate { get; set; }
        
        [StringLength(1000)]
        public string NotificationMethod { get; set; } = string.Empty; // Email, SMS, InApp, Website
        
        public DateTime? ResolvedDate { get; set; }
        
        [StringLength(100)]
        public string ResolvedBy { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string LessonsLearned { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string PreventiveMeasures { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string SourceIpAddress { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string SourceUserAgent { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string TechnicalDetails { get; set; } = string.Empty; // JSON with technical information
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;
    }
}