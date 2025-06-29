using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TriathlonTracker.Models.Base;
using TriathlonTracker.Models.Enums;
using System.Text.Json;

namespace TriathlonTracker.Models
{
    public class IpAccessControl : BaseEntityWithMetadata
    {
        [Required]
        [StringLength(45)] // IPv6 max length
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? IpRange { get; set; }
        
        [Required]
        public string AccessType { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? ExpiresAt { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        public int HitCount { get; set; } = 0;
        
        public DateTime? LastHit { get; set; }
        
        [StringLength(100)]
        public string? Country { get; set; }
        
        [StringLength(100)]
        public string? Region { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
    }

    public class SecurityEvent : BaseEntityWithMetadata
    {
        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty;
        
        [Required]
        public string Severity { get; set; } = "Medium";
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [StringLength(450)]
        public string? UserId { get; set; }
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        [StringLength(100)]
        public string? SessionId { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Source { get; set; }
        
        public bool IsResolved { get; set; } = false;
        
        [StringLength(1000)]
        public string? ResolutionNotes { get; set; }
        
        [StringLength(100)]
        public string? ResolvedBy { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
        
        public Dictionary<string, object> EventData { get; set; } = new();
        
        [NotMapped]
        public string EventDataJson
        {
            get => JsonSerializer.Serialize(EventData);
            set => EventData = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public class EncryptionKey : BaseEntityWithStringMetadata
    {
        [Required]
        [StringLength(100)]
        public string KeyName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string KeyType { get; set; } = string.Empty; // AES, RSA, etc.
        
        [Required]
        public string EncryptedKey { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string KeyHash { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime? ExpiresAt { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        public int UsageCount { get; set; } = 0;
        
        public DateTime? LastUsed { get; set; }
        
        [StringLength(200)]
        public string Purpose { get; set; } = string.Empty;
        
        public Dictionary<string, object> KeyMetadata { get; set; } = new();
        
        [NotMapped]
        public string KeyMetadataJson
        {
            get => JsonSerializer.Serialize(KeyMetadata);
            set => KeyMetadata = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public class AccessAttempt : BaseEntityWithMetadata
    {
        [StringLength(450)]
        public string? UserId { get; set; }
        
        [StringLength(100)]
        public string? Username { get; set; }
        
        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        [Required]
        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;
        
        [Required]
        public bool IsSuccessful { get; set; }
        
        [StringLength(500)]
        public string? FailureReason { get; set; }
        
        [StringLength(200)]
        public string Resource { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string Method { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? SessionId { get; set; }
        
        [StringLength(500)]
        public string? ReferrerUrl { get; set; }
        
        public bool IsSuspicious { get; set; } = false;
        
        [StringLength(500)]
        public string? SuspiciousReason { get; set; }
        
        public Dictionary<string, object> AdditionalData { get; set; } = new();
        
        [NotMapped]
        public string AdditionalDataJson
        {
            get => JsonSerializer.Serialize(AdditionalData);
            set => AdditionalData = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public class ThreatIntelligence : BaseEntityWithMetadata
    {
        [Required]
        [StringLength(100)]
        public string ThreatType { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Indicator { get; set; } = string.Empty; // IP, Domain, Hash, etc.
        
        [Required]
        [StringLength(50)]
        public string IndicatorType { get; set; } = string.Empty;
        
        public string Severity { get; set; } = "Medium";
        
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(100)]
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
            get => JsonSerializer.Serialize(ThreatData);
            set => ThreatData = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }
}