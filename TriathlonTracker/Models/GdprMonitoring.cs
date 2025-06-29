using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriathlonTracker.Models
{
    public class GdprAuditLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Action { get; set; } = string.Empty;
        
        [Required]
        public string EntityType { get; set; } = string.Empty;
        
        public string? EntityId { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public string? AdminUserId { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string? IpAddress { get; set; }
        
        public string? UserAgent { get; set; }
        
        public string? Details { get; set; }
        
        public string? OldValues { get; set; }
        
        public string? NewValues { get; set; }
        
        public bool IsSuccessful { get; set; } = true;
        
        public string? ErrorMessage { get; set; }
        
        public string? SessionId { get; set; }
        
        public string? RequestId { get; set; }
    }

    public class ConsentAnalytics
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public DateTime Date { get; set; }
        
        public int TotalConsents { get; set; }
        
        public int NewConsents { get; set; }
        
        public int RevokedConsents { get; set; }
        
        public int UpdatedConsents { get; set; }
        
        public string ConsentType { get; set; } = string.Empty;
        
        public double ConsentRate { get; set; }
        
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

    public class DataProcessingAnalytics
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
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

    public class ComplianceMonitor
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string MonitorType { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public int CheckIntervalMinutes { get; set; } = 60;
        
        public DateTime LastCheck { get; set; }
        
        public DateTime NextCheck { get; set; }
        
        public string Status { get; set; } = "OK";
        
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

    public class SuspiciousActivity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string ActivityType { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public string? IpAddress { get; set; }
        
        public string? UserAgent { get; set; }
        
        [Required]
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        
        public string Description { get; set; } = string.Empty;
        
        public string Severity { get; set; } = "Medium";
        
        public bool IsInvestigated { get; set; } = false;
        
        public string? InvestigationNotes { get; set; }
        
        public string? InvestigatedBy { get; set; }
        
        public DateTime? InvestigatedAt { get; set; }
        
        public string Status { get; set; } = "Open";
        
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        [NotMapped]
        public string MetadataJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(Metadata);
            set => Metadata = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public class SessionMonitoring
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string SessionId { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public string? IpAddress { get; set; }
        
        public string? UserAgent { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndTime { get; set; }
        
        public int ActivityCount { get; set; } = 0;
        
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public string? Location { get; set; }
        
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