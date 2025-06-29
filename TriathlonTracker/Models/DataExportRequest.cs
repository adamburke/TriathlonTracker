using System.ComponentModel.DataAnnotations;
using TriathlonTracker.Models.Base;
using TriathlonTracker.Models.Enums;

namespace TriathlonTracker.Models
{
    public class DataExportRequest : BaseEntity
    {
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string Format { get; set; } = "JSON"; // JSON, CSV, XML, PDF
        
        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        
        public string Status { get; set; } = "Pending";
        
        public DateTime? CompletedDate { get; set; }
        
        public DateTime? ExpirationDate { get; set; }
        
        [StringLength(500)]
        public string DownloadUrl { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string FileName { get; set; } = string.Empty;
        
        public long FileSizeBytes { get; set; }
        
        [StringLength(500)]
        public string ErrorMessage { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string IpAddress { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string UserAgent { get; set; } = string.Empty;
        
        public bool IsNotificationSent { get; set; } = false;
        
        public int DownloadCount { get; set; } = 0;
        
        public DateTime? LastDownloadDate { get; set; }
        
        // Navigation Properties
        public User? User { get; set; }
    }
}