using System.ComponentModel.DataAnnotations;
using TriathlonTracker.Models.Base;
using TriathlonTracker.Models.Enums;

namespace TriathlonTracker.Models
{
    public class SecurityStatus : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string OverallStatus { get; set; } = string.Empty;
        
        public DateTime LastSecurityScan { get; set; }
        
        public int ActiveThreats { get; set; }
        
        public int SecurityScore { get; set; }
        
        public int RecentIncidents { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SystemHealth { get; set; } = string.Empty;
        
        public DateTime LastUpdated { get; set; }
        
        // Additional properties for services
        public bool IsSystemSecure { get; set; } = true;
        
        public DateTime LastSecurityCheck { get; set; } = DateTime.UtcNow;
        
        public List<string> Recommendations { get; set; } = new();
    }

    public class ComplianceMetrics : BaseEntity
    {
        public int TotalUsers { get; set; }
        
        public int ConsentedUsers { get; set; }
        
        public int PendingRequests { get; set; }
        
        public double ComplianceScore { get; set; }
        
        public int DataProcessingActivities { get; set; }
        
        public int RetentionPoliciesActive { get; set; }
        
        public DateTime LastCalculated { get; set; }
    }

    public class AuditLogEntry
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string Details { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
    }

    public class SecurityMetrics : BaseEntity
    {
        public int TotalSecurityEvents { get; set; }
        
        public int HighSeverityEvents { get; set; }
        
        public int BlockedIpAddresses { get; set; }
        
        public int FailedLoginAttempts { get; set; }
        
        public int SuspiciousActivities { get; set; }
        
        public int SecurityScore { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public DateTime GeneratedAt { get; set; }
    }

    public class RetentionSummary : BaseEntity
    {
        public int TotalPolicies { get; set; }
        
        public int ActivePolicies { get; set; }
        
        public int PendingJobs { get; set; }
        
        public int CompletedJobs { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TotalDataRetained { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string DataEligibleForDeletion { get; set; } = string.Empty;
        
        public DateTime LastRetentionRun { get; set; }
        
        public DateTime NextScheduledRun { get; set; }
    }

    public class ExpiredDataSummary : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string DataType { get; set; } = string.Empty;
        
        public int RecordCount { get; set; }
        
        public DateTime OldestRecord { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TotalSize { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string RetentionPolicy { get; set; } = string.Empty;
        
        public bool EligibleForDeletion { get; set; }
    }

    public class SecurityStatusInfo : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string OverallRisk { get; set; } = "Low";
        
        public int ActiveSessions { get; set; }
        
        public int ThreatLevel { get; set; }
        
        public DateTime LastSecurityScan { get; set; }
    }

    public class ExpiredData : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string DataType { get; set; } = string.Empty;
        
        public int RecordCount { get; set; }
        
        public DateTime ExpirationDate { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PolicyName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string TotalSize { get; set; } = string.Empty;
        
        public bool RequiresAction { get; set; } = true;
    }
}