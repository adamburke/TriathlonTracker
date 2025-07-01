using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public interface ITelemetryService
    {
        Task<bool> IsEnabledAsync();
        Task TrackEventAsync(string eventName, string eventCategory, string? userId = null, Dictionary<string, object>? properties = null, Dictionary<string, object>? metrics = null);
        Task TrackApiRequestAsync(string endpoint, string httpMethod, int statusCode, long responseTimeMs, long requestSize, long responseSize, string? userId = null);
        Task TrackExceptionAsync(Exception exception, string? userId = null, Dictionary<string, object>? properties = null);
        Task TrackUserActionAsync(string action, string? userId = null, Dictionary<string, object>? properties = null);
        Task TrackPerformanceMetricAsync(string metricName, double value, string? userId = null, Dictionary<string, object>? properties = null);
        List<TelemetryEvent> GetQueuedEvents();
        List<TelemetryEvent> GetEventsByRequestId(string requestId);
        int GetQueueSize();
    }
} 