using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TriathlonTracker.Models.Base;
using TriathlonTracker.Models.Enums;

namespace TriathlonTracker.Models
{
    public class RetentionJob : BaseEntityWithMetadata
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        public string JobType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string DataType { get; set; } = string.Empty;
        
        public bool IsEnabled { get; set; } = true;
        
        [Required]
        [StringLength(100)]
        public string Schedule { get; set; } = string.Empty; // Cron expression
        
        public DateTime? LastRun { get; set; }
        
        public DateTime? NextRun { get; set; }
        
        public string Status { get; set; } = "Pending";
        
        public int ProcessedRecords { get; set; } = 0;
        
        public int FailedRecords { get; set; } = 0;
        
        [StringLength(1000)]
        public string? LastError { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        public Dictionary<string, object> Configuration { get; set; } = new();
        
        [NotMapped]
        public string ConfigurationJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(Configuration);
            set => Configuration = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public class RetentionJobExecution : BaseEntityWithMetadata
    {
        [Required]
        [StringLength(450)]
        public string JobId { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndTime { get; set; }
        
        public string Status { get; set; } = "Processing";
        
        public int ProcessedRecords { get; set; } = 0;
        
        public int SuccessfulRecords { get; set; } = 0;
        
        public int FailedRecords { get; set; } = 0;
        
        [StringLength(1000)]
        public string? ErrorMessage { get; set; }
        
        public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
        
        // Navigation property
        public RetentionJob? Job { get; set; }
        
        public Dictionary<string, object> ExecutionDetails { get; set; } = new();
        
        [NotMapped]
        public string ExecutionDetailsJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(ExecutionDetails);
            set => ExecutionDetails = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public class DataArchive : BaseEntityWithStringMetadata
    {
        [Required]
        [StringLength(100)]
        public string OriginalEntityType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(450)]
        public string OriginalEntityId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public string ArchivedData { get; set; } = string.Empty; // JSON serialized data
        
        [Required]
        public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime OriginalCreatedAt { get; set; }
        
        [StringLength(500)]
        public string ArchiveReason { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string ArchivedBy { get; set; } = string.Empty;
        
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsEncrypted { get; set; } = true;
        
        [StringLength(100)]
        public string? EncryptionKey { get; set; }
        
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

    public class RetentionNotification : BaseEntityWithMetadata
    {
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        public string NotificationType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string DataType { get; set; } = string.Empty;
        
        public DateTime ExpirationDate { get; set; }
        
        public DateTime? SentAt { get; set; }
        
        public bool IsSent { get; set; } = false;
        
        [StringLength(100)]
        public string? DeliveryStatus { get; set; }
        
        [StringLength(500)]
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

    public class RetentionAuditTrail : BaseEntityWithMetadata
    {
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // Archived, Deleted, Notified
        
        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(450)]
        public string EntityId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
        
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string PerformedBy { get; set; } = string.Empty;
        
        [StringLength(450)]
        public string? JobId { get; set; }
        
        // Navigation properties
        public RetentionJob? Job { get; set; }
        
        public Dictionary<string, object> ActionDetails { get; set; } = new();
        
        [NotMapped]
        public string ActionDetailsJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(ActionDetails);
            set => ActionDetails = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }
}