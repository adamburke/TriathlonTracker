using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TriathlonTracker.Models
{
    public class IpAccessControl
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string IpAddress { get; set; } = string.Empty;
        
        public string? IpRange { get; set; }
        
        [Required]
        public string AccessType { get; set; } = string.Empty; // Allow, Block, Monitor
        
        public string Reason { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExpiresAt { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public int HitCount { get; set; } = 0;
        
        public DateTime? LastHit { get; set; }
        
        public string? Country { get; set; }
        
        public string? Region { get; set; }
        
        public string? City { get; set; }
        
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

    public class SecurityEvent
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string EventType { get; set; } = string.Empty;
        
        [Required]
        public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public string? UserId { get; set; }
        
        public string? IpAddress { get; set; }
        
        public string? UserAgent { get; set; }
        
        public string? SessionId { get; set; }
        
        public string Description { get; set; } = string.Empty;
        
        public string? Source { get; set; }
        
        public bool IsResolved { get; set; } = false;
        
        public string? ResolutionNotes { get; set; }
        
        public string? ResolvedBy { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
        
        public Dictionary<string, object> EventData { get; set; } = new();
        
        [NotMapped]
        public string EventDataJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(EventData);
            set => EventData = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public class EncryptionKey
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string KeyName { get; set; } = string.Empty;
        
        [Required]
        public string KeyType { get; set; } = string.Empty; // AES, RSA, etc.
        
        [Required]
        public string EncryptedKey { get; set; } = string.Empty;
        
        [Required]
        public string KeyHash { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ExpiresAt { get; set; }
        
        public string CreatedBy { get; set; } = string.Empty;
        
        public int UsageCount { get; set; } = 0;
        
        public DateTime? LastUsed { get; set; }
        
        public string Purpose { get; set; } = string.Empty;
        
        public Dictionary<string, string> KeyMetadata { get; set; } = new();
        
        [NotMapped]
        public string KeyMetadataJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(KeyMetadata);
            set => KeyMetadata = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, string>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new();
        }
    }

    public class AccessAttempt
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public string? UserId { get; set; }
        
        public string? Username { get; set; }
        
        [Required]
        public string IpAddress { get; set; } = string.Empty;
        
        public string? UserAgent { get; set; }
        
        [Required]
        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;
        
        [Required]
        public bool IsSuccessful { get; set; }
        
        public string? FailureReason { get; set; }
        
        public string Resource { get; set; } = string.Empty;
        
        public string Method { get; set; } = string.Empty;
        
        public string? SessionId { get; set; }
        
        public string? ReferrerUrl { get; set; }
        
        public bool IsSuspicious { get; set; } = false;
        
        public string? SuspiciousReason { get; set; }
        
        public Dictionary<string, object> AdditionalData { get; set; } = new();
        
        [NotMapped]
        public string AdditionalDataJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(AdditionalData);
            set => AdditionalData = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public class ThreatIntelligence
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string ThreatType { get; set; } = string.Empty;
        
        [Required]
        public string Indicator { get; set; } = string.Empty; // IP, Domain, Hash, etc.
        
        [Required]
        public string IndicatorType { get; set; } = string.Empty;
        
        public string Severity { get; set; } = "Medium";
        
        public string Description { get; set; } = string.Empty;
        
        public string Source { get; set; } = string.Empty;
        
        public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
        
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? ExpiresAt { get; set; }
        
        public int HitCount { get; set; } = 0;
        
        public Dictionary<string, object> ThreatData { get; set; } = new();
        
        [NotMapped]
        public string ThreatDataJson
        {
            get => System.Text.Json.JsonSerializer.Serialize(ThreatData);
            set => ThreatData = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }
}