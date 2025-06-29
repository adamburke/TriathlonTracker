using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriathlonTracker.Models
{
    public class RetentionJob
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string JobType { get; set; } = string.Empty; // Cleanup, Archive, Notify
        
        [Required]
        public string DataType { get; set; } = string.Empty;
        
        public bool IsEnabled { get; set; } = true;
        
        [Required]
        public string Schedule { get; set; } = string.Empty; // Cron expression
        
        public DateTime? LastRun { get; set; }
        
        public DateTime? NextRun { get; set; }
        
        public string Status { get; set; } = "Scheduled";
        
        public int ProcessedRecords { get; set; } = 0;
        
        public int FailedRecords { get; set; } = 0;
        
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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class RetentionJobExecution
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string JobId { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndTime { get; set; }
        
        public string Status { get; set; } = "Running";
        
        public int ProcessedRecords { get; set; } = 0;
        
        public int SuccessfulRecords { get; set; } = 0;
        
        public int FailedRecords { get; set; } = 0;
        
        public string? ErrorMessage { get; set; }
        
        public Dictionary<string, object> ExecutionDetails { get; set; } = new();
        
        [NotMapped]
        public string ExecutionDetailsJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(ExecutionDetails);
            set => ExecutionDetails = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
        
        public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
        
        // Navigation property
        public RetentionJob? Job { get; set; }
    }

    public class DataArchive
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string OriginalEntityType { get; set; } = string.Empty;
        
        [Required]
        public string OriginalEntityId { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string ArchivedData { get; set; } = string.Empty; // JSON serialized data
        
        [Required]
        public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime OriginalCreatedAt { get; set; }
        
        public string ArchiveReason { get; set; } = string.Empty;
        
        public string ArchivedBy { get; set; } = string.Empty;
        
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsEncrypted { get; set; } = true;
        
        public string? EncryptionKey { get; set; }
        
        public Dictionary<string, string> Metadata { get; set; } = new();
        
        [NotMapped]
        public string MetadataJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(Metadata);
            set => Metadata = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, string>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new();
        }
    }

    public class RetentionNotification
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string NotificationType { get; set; } = string.Empty; // DataExpiring, DataExpired, DataArchived
        
        [Required]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        public string DataType { get; set; } = string.Empty;
        
        public DateTime ExpirationDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? SentAt { get; set; }
        
        public bool IsSent { get; set; } = false;
        
        public string? DeliveryStatus { get; set; }
        
        public string? DeliveryError { get; set; }
        
        public int RetryCount { get; set; } = 0;
        
        public DateTime? NextRetry { get; set; }
        
        public Dictionary<string, object> NotificationData { get; set; } = new();
        
        [NotMapped]
        public string NotificationDataJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(NotificationData);
            set => NotificationData = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public class RetentionAuditTrail
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Action { get; set; } = string.Empty; // Archived, Deleted, Notified
        
        [Required]
        public string EntityType { get; set; } = string.Empty;
        
        [Required]
        public string EntityId { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
        
        public string Reason { get; set; } = string.Empty;
        
        public string PerformedBy { get; set; } = string.Empty;
        
        public string? JobId { get; set; }
        
        public Dictionary<string, object> ActionDetails { get; set; } = new();
        
        [NotMapped]
        public string ActionDetailsJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(ActionDetails);
            set => ActionDetails = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
        
        // Navigation properties
        public RetentionJob? Job { get; set; }
    }
}