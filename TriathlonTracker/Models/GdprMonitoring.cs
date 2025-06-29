using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TriathlonTracker.Models.Base;
using TriathlonTracker.Models.Enums;

namespace TriathlonTracker.Models
{
    public class GdprAuditLog : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;
        
        [StringLength(450)]
        public string? EntityId { get; set; }
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [StringLength(450)]
        public string? AdminUserId { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        [StringLength(1000)]
        public string? Details { get; set; }
        
        [StringLength(2000)]
        public string? OldValues { get; set; }
        
        [StringLength(2000)]
        public string? NewValues { get; set; }
        
        public bool IsSuccessful { get; set; } = true;
        
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }
        
        [StringLength(100)]
        public string? SessionId { get; set; }
        
        [StringLength(100)]
        public string? RequestId { get; set; }
    }

    public class ConsentAnalytics : BaseEntityWithMetadata
    {
        [Required]
        public DateTime Date { get; set; }
        
        public int TotalConsents { get; set; }
        
        public int NewConsents { get; set; }
        
        public int RevokedConsents { get; set; }
        
        public int UpdatedConsents { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ConsentType { get; set; } = string.Empty;
        
        public double ConsentRate { get; set; }
        
        [StringLength(100)]
        public string? Source { get; set; }
        
        public Dictionary<string, int> ConsentByPurpose { get; set; } = new();
        
        [NotMapped]
        public string ConsentByPurposeJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(ConsentByPurpose);
            set => ConsentByPurpose = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, int>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(value) ?? new();
        }
    }

    public class DataProcessingAnalytics : BaseEntityWithMetadata
    {
        [Required]
        public DateTime Date { get; set; }
        
        public int TotalProcessingActivities { get; set; }
        
        public int DataAccessRequests { get; set; }
        
        public int DataExportRequests { get; set; }
        
        public int DataDeletionRequests { get; set; }
        
        public int DataRectificationRequests { get; set; }
        
        public double AverageResponseTime { get; set; }
        
        public int FailedRequests { get; set; }
        
        public Dictionary<string, int> ProcessingByType { get; set; } = new();
        
        [NotMapped]
        public string ProcessingByTypeJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(ProcessingByType);
            set => ProcessingByType = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, int>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(value) ?? new();
        }
    }

    public class ComplianceMonitor : BaseEntityWithMetadata
    {
        [Required]
        [StringLength(100)]
        public string MonitorType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public int CheckIntervalMinutes { get; set; } = 60;
        
        public DateTime LastCheck { get; set; }
        
        public DateTime NextCheck { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "OK";
        
        [StringLength(1000)]
        public string? LastError { get; set; }
        
        public Dictionary<string, object> Configuration { get; set; } = new();
        
        [NotMapped]
        public string ConfigurationJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(Configuration);
            set => Configuration = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
        
        public Dictionary<string, object> LastResult { get; set; } = new();
        
        [NotMapped]
        public string LastResultJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(LastResult);
            set => LastResult = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public class SuspiciousActivity : BaseEntityWithMetadata
    {
        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        [Required]
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public string Severity { get; set; } = "Medium";
        
        public bool IsInvestigated { get; set; } = false;
        
        [StringLength(1000)]
        public string? InvestigationNotes { get; set; }
        
        [StringLength(100)]
        public string? InvestigatedBy { get; set; }
        
        public DateTime? InvestigatedAt { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Open";
    }

    public class SessionMonitoring : BaseEntityWithMetadata
    {
        [Required]
        [StringLength(100)]
        public string SessionId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndTime { get; set; }
        
        public int ActivityCount { get; set; } = 0;
        
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        [StringLength(200)]
        public string? Device { get; set; }
        
        public List<string> AccessedResources { get; set; } = new();
        
        [NotMapped]
        public string AccessedResourcesJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(AccessedResources);
            set => AccessedResources = string.IsNullOrEmpty(value) 
                ? new List<string>() 
                : System.Text.Json.JsonSerializer.Deserialize<List<string>>(value) ?? new();
        }
    }
}