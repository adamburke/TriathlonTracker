using System.Text.Json;

namespace TriathlonTracker.Models
{
    public class TelemetryEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EventName { get; set; } = string.Empty;
        public string EventCategory { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? SessionId { get; set; }
        public string? CorrelationId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? RequestId { get; set; }
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public int? StatusCode { get; set; }
        public long? ResponseTimeMs { get; set; }
        public long? RequestSizeBytes { get; set; }
        public long? ResponseSizeBytes { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorType { get; set; }
        public string? StackTrace { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
        public Dictionary<string, object>? Metrics { get; set; }
        public string Environment { get; set; } = "Development";
        public string ApplicationVersion { get; set; } = "1.0.0";
        public string Source { get; set; } = "TriathlonTracker";

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }
    }

    public static class TelemetryEventNames
    {
        public const string ApiRequest = "ApiRequest";
        public const string UserAction = "UserAction";
        public const string UnhandledException = "UnhandledException";
        public const string PerformanceMetric = "PerformanceMetric";
        public const string BusinessEvent = "BusinessEvent";
    }

    public static class TelemetryEventCategories
    {
        public const string Api = "Api";
        public const string Business = "Business";
        public const string Error = "Error";
        public const string Performance = "Performance";
        public const string User = "User";
    }
} 