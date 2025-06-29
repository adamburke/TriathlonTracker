using System.ComponentModel.DataAnnotations;
using TriathlonTracker.Models.Base;
using TriathlonTracker.Models.Enums;

namespace TriathlonTracker.Models
{
    public class AccountDeletionRequest : BaseEntity
    {
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        
        public string Status { get; set; } = "Pending";
        
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;
        
        [Required]
        public DeletionType DeletionType { get; set; } = DeletionType.SoftDelete;
        
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