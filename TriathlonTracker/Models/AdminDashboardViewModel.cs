using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class AdminDashboardViewModel
    {
        public GdprComplianceMetrics ComplianceMetrics { get; set; } = new();
        public List<UserGdprStatus> UserStatuses { get; set; } = new();
        public List<RecentActivity> RecentActivities { get; set; } = new();
        public List<ComplianceAlert> Alerts { get; set; } = new();
        public DataRetentionSummary RetentionSummary { get; set; } = new();
    }

    public class GdprComplianceMetrics
    {
        public int TotalUsers { get; set; }
        public int UsersWithConsent { get; set; }
        public int PendingDataRequests { get; set; }
        public int CompletedDataRequests { get; set; }
        public int ActiveBreachIncidents { get; set; }
        public int ResolvedBreachIncidents { get; set; }
        public double ConsentRate => TotalUsers > 0 ? (double)UsersWithConsent / TotalUsers * 100 : 0;
        public int DataRetentionViolations { get; set; }
        public DateTime LastComplianceCheck { get; set; }
    }

    public class UserGdprStatus
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool HasConsent { get; set; }
        public DateTime? ConsentDate { get; set; }
        public DateTime? LastDataAccess { get; set; }
        public int PendingRequests { get; set; }
        public bool HasRetentionViolation { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class RecentActivity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ComplianceAlert
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsResolved { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class DataRetentionSummary
    {
        public int TotalRecords { get; set; }
        public int RecordsNearExpiry { get; set; }
        public int ExpiredRecords { get; set; }
        public int ArchivedRecords { get; set; }
        public DateTime NextCleanupDate { get; set; }
        public List<RetentionPolicyStatus> PolicyStatuses { get; set; } = new();
    }

    public class RetentionPolicyStatus
    {
        public string PolicyName { get; set; } = string.Empty;
        public int AffectedRecords { get; set; }
        public DateTime LastExecuted { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class BulkConsentOperation
    {
        [Required]
        public string Operation { get; set; } = string.Empty; // Grant, Revoke, Update
        
        [Required]
        public List<string> UserIds { get; set; } = new();
        
        public string? ConsentType { get; set; }
        public string? Reason { get; set; }
        public bool NotifyUsers { get; set; } = true;
    }

    public class ComplianceReport
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public string Status { get; set; } = "Generated";
    }
}