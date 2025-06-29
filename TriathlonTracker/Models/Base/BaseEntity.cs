using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TriathlonTracker.Models.Base
{
    public abstract class BaseEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public abstract class BaseEntityWithMetadata : BaseEntity
    {
        public Dictionary<string, object> Metadata { get; set; } = new();
        
        [NotMapped]
        public string MetadataJson
        {
            get => JsonSerializer.Serialize(Metadata);
            set => Metadata = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, object>() 
                : JsonSerializer.Deserialize<Dictionary<string, object>>(value) ?? new();
        }
    }

    public abstract class BaseEntityWithStringMetadata : BaseEntity
    {
        public Dictionary<string, string> StringMetadata { get; set; } = new();
        
        [NotMapped]
        public string StringMetadataJson
        {
            get => JsonSerializer.Serialize(StringMetadata);
            set => StringMetadata = string.IsNullOrEmpty(value) 
                ? new Dictionary<string, string>() 
                : JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new();
        }
    }
} 