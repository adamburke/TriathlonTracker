using System.ComponentModel.DataAnnotations;
using TriathlonTracker.Models.Base;
using TriathlonTracker.Models.Enums;

namespace TriathlonTracker.Models
{
    public class GdprComplianceMetrics : BaseEntity
    {
        public int TotalUsers { get; set; }
        public int UsersWithConsent { get; set; }
        public int PendingDataRequests { get; set; }
        public int CompletedDataRequests { get; set; }
        public int ActiveBreachIncidents { get; set; }
        public int ResolvedBreachIncidents { get; set; }
        public int DataRetentionViolations { get; set; }
        public DateTime LastComplianceCheck { get; set; }
        
        public double ConsentRate => TotalUsers > 0 ? (double)UsersWithConsent / TotalUsers * 100 : 0;
    }

    public class UserGdprStatus : BaseEntity
    {
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        public bool HasConsent { get; set; }
        public DateTime? ConsentDate { get; set; }
        public DateTime? LastDataAccess { get; set; }
        public int PendingRequests { get; set; }
        public bool HasRetentionViolation { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class RecentActivity : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string UserEmail { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
    }

    public class ComplianceAlert : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Type { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Severity { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
    }

    public class DataRetentionSummary : BaseEntity
    {
        public int TotalRecords { get; set; }
        public int RecordsEligibleForDeletion { get; set; }
        public int ActiveRetentionPolicies { get; set; }
        public int PendingCleanupJobs { get; set; }
        public DateTime LastCleanupRun { get; set; }
        public DateTime NextScheduledCleanup { get; set; }
        
        // Additional properties for compatibility
        public int RecordsNearExpiry { get; set; }
        public int ExpiredRecords { get; set; }
        public int ArchivedRecords { get; set; }
        public DateTime NextCleanupDate { get; set; }
        public List<RetentionPolicyStatus> PolicyStatuses { get; set; } = new();
    }

    public class RetentionPolicyStatus : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string PolicyName { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        public int RecordsAffected { get; set; }
        public DateTime LastExecution { get; set; }
        public DateTime NextExecution { get; set; }
        public string Status { get; set; } = string.Empty;
        
        // Additional properties for compatibility
        public int AffectedRecords { get; set; }
        public DateTime LastExecuted { get; set; }
    }

    public class BulkConsentOperation : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string OperationType { get; set; } = string.Empty;
        
        [Required]
        public List<string> UserIds { get; set; } = new();
        
        [Required]
        [StringLength(100)]
        public string ConsentType { get; set; } = string.Empty;
        
        public bool IsGranted { get; set; }
        
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        public DateTime ScheduledDate { get; set; }
        
        // Additional properties for compatibility
        [Required]
        public string Operation { get; set; } = string.Empty; // Grant, Revoke, Update
        public bool NotifyUsers { get; set; } = true;
    }

    public class ComplianceReport : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string ReportType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        
        [StringLength(100)]
        public string GeneratedBy { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;
        
        // Additional properties for compatibility
        public string Type { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
    }
} 