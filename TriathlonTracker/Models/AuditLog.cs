using System;
using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string? UserId { get; set; }
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        [MaxLength(1000)]
        public string? Details { get; set; }
        [MaxLength(50)]
        public string? IpAddress { get; set; }
        [MaxLength(200)]
        public string? UserAgent { get; set; }
        [MaxLength(20)]
        public string? LogLevel { get; set; } = "Information";
    }
} 