using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class SecurityStatus
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OverallStatus { get; set; } = string.Empty;
        public DateTime LastSecurityScan { get; set; }
        public int ActiveThreats { get; set; }
        public int SecurityScore { get; set; }
        public int RecentIncidents { get; set; }
        public string SystemHealth { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class ComplianceMetrics
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
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

    public class SecurityMetrics
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
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

    public class RetentionSummary
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public int PendingJobs { get; set; }
        public int CompletedJobs { get; set; }
        public string TotalDataRetained { get; set; } = string.Empty;
        public string DataEligibleForDeletion { get; set; } = string.Empty;
        public DateTime LastRetentionRun { get; set; }
        public DateTime NextScheduledRun { get; set; }
    }

    public class ExpiredDataSummary
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DataType { get; set; } = string.Empty;
        public int RecordCount { get; set; }
        public DateTime OldestRecord { get; set; }
        public string TotalSize { get; set; } = string.Empty;
        public string RetentionPolicy { get; set; } = string.Empty;
        public bool EligibleForDeletion { get; set; }
    }
    public class SecurityStatusInfo
    {
        public string OverallRisk { get; set; } = "Low";
        public int ActiveSessions { get; set; }
        public int ThreatLevel { get; set; }
        public DateTime LastSecurityScan { get; set; }
    }

    public class ExpiredData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DataType { get; set; } = string.Empty;
        public int RecordCount { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string TotalSize { get; set; } = string.Empty;
        public bool RequiresAction { get; set; } = true;
    }
}