using System.ComponentModel.DataAnnotations;

namespace TriathlonTracker.Models
{
    public class Configuration
    {
        [Key]
        public string Key { get; set; } = string.Empty;
        
        public string Value { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public bool IsEncrypted { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}