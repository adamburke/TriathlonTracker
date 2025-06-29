using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class AccountDeletionRequest
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Processing, Completed, Cancelled
        
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string DeletionType { get; set; } = "SoftDelete"; // SoftDelete, HardDelete, Anonymize
        
        public DateTime? ConfirmationDate { get; set; }
        
        public DateTime? ScheduledDeletionDate { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        [StringLength(100)]
        public string ProcessedBy { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string ConfirmationToken { get; set; } = string.Empty;
        
        public DateTime? TokenExpirationDate { get; set; }
        
        [StringLength(200)]
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        public bool IsNotificationSent { get; set; } = false;
        
        public bool IsRecoveryPeriodActive { get; set; } = true;
        
        public int RecoveryPeriodDays { get; set; } = 30; // Days user can recover account
        
        public DateTime? RecoveryDeadline { get; set; }
        
        [StringLength(1000)]
        public string ProcessingNotes { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string DataBackupInfo { get; set; } = string.Empty; // JSON info about backed up data
        
        public bool HasDataExport { get; set; } = false;
        
        public DateTime? DataExportDate { get; set; }
        
        // Navigation Properties
        public User? User { get; set; }
    }
}