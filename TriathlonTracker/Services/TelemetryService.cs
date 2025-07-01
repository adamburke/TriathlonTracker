using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public class TelemetryService : ITelemetryService
    {
        private readonly ILogger<TelemetryService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Queue<TelemetryEvent> _eventQueue;
        private readonly object _queueLock = new object();

        public TelemetryService(
            ILogger<TelemetryService> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _eventQueue = new Queue<TelemetryEvent>();
        }

        public Task<bool> IsEnabledAsync()
        {
            try
            {
                var enabled = _configuration.GetValue<bool>("Telemetry:Enabled", false);
                return Task.FromResult(enabled);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking telemetry enabled status");
                return Task.FromResult(false);
            }
        }

        public async Task TrackEventAsync(string eventName, string eventCategory, string? userId = null, Dictionary<string, object>? properties = null, Dictionary<string, object>? metrics = null)
        {
            if (!await IsEnabledAsync())
                return;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var requestId = httpContext?.TraceIdentifier;
                var telemetryEvent = new TelemetryEvent
                {
                    EventName = eventName,
                    EventCategory = eventCategory,
                    UserId = userId,
                    SessionId = GetSessionId(httpContext),
                    CorrelationId = requestId,
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                    UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                    RequestId = requestId,
                    Properties = properties,
                    Metrics = metrics,
                    Environment = _configuration["Environment"] ?? "Development",
                    ApplicationVersion = _configuration["ApplicationVersion"] ?? "1.0.0"
                };

                EnqueueEvent(telemetryEvent);
                _logger.LogDebug(
                    "Queued telemetry event - RequestId: {RequestId}, Event: {EventName}, Category: {Category}, UserId: {UserId}",
                    requestId,
                    eventName,
                    eventCategory,
                    userId ?? "anonymous"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking telemetry event: {EventName}", eventName);
            }
        }

        public async Task TrackApiRequestAsync(string endpoint, string httpMethod, int statusCode, long responseTimeMs, long requestSize, long responseSize, string? userId = null)
        {
            if (!await IsEnabledAsync())
                return;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var requestId = httpContext?.TraceIdentifier;
                var telemetryEvent = new TelemetryEvent
                {
                    EventName = TelemetryEventNames.ApiRequest,
                    EventCategory = TelemetryEventCategories.Api,
                    UserId = userId,
                    SessionId = GetSessionId(httpContext),
                    CorrelationId = requestId,
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                    UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                    RequestId = requestId,
                    Endpoint = endpoint,
                    HttpMethod = httpMethod,
                    StatusCode = statusCode,
                    ResponseTimeMs = responseTimeMs,
                    RequestSizeBytes = requestSize,
                    ResponseSizeBytes = responseSize,
                    Environment = _configuration["Environment"] ?? "Development",
                    ApplicationVersion = _configuration["ApplicationVersion"] ?? "1.0.0"
                };

                EnqueueEvent(telemetryEvent);

                // Log slow requests with correlation details
                if (responseTimeMs > 1000)
                {
                    _logger.LogWarning(
                        "Slow API request detected - RequestId: {RequestId}, Endpoint: {Endpoint}, Duration: {ResponseTime}ms, UserId: {UserId}",
                        requestId,
                        endpoint,
                        responseTimeMs,
                        userId ?? "anonymous"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking API request telemetry");
            }
        }

        public async Task TrackExceptionAsync(Exception exception, string? userId = null, Dictionary<string, object>? properties = null)
        {
            if (!await IsEnabledAsync())
                return;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var requestId = httpContext?.TraceIdentifier;
                var telemetryEvent = new TelemetryEvent
                {
                    EventName = TelemetryEventNames.UnhandledException,
                    EventCategory = TelemetryEventCategories.Error,
                    UserId = userId,
                    SessionId = GetSessionId(httpContext),
                    CorrelationId = requestId,
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                    UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                    RequestId = requestId,
                    Endpoint = $"{httpContext?.Request?.Path}{httpContext?.Request?.QueryString}",
                    HttpMethod = httpContext?.Request?.Method,
                    StatusCode = httpContext?.Response?.StatusCode,
                    ErrorMessage = exception.Message,
                    ErrorType = exception.GetType().Name,
                    StackTrace = exception.StackTrace,
                    Properties = properties,
                    Environment = _configuration["Environment"] ?? "Development",
                    ApplicationVersion = _configuration["ApplicationVersion"] ?? "1.0.0"
                };

                EnqueueEvent(telemetryEvent);
                _logger.LogError(
                    exception,
                    "Queued exception telemetry - RequestId: {RequestId}, ErrorType: {ErrorType}, UserId: {UserId}, Endpoint: {Endpoint}",
                    requestId,
                    exception.GetType().Name,
                    userId ?? "anonymous",
                    telemetryEvent.Endpoint
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking exception telemetry");
            }
        }

        public async Task TrackUserActionAsync(string action, string? userId = null, Dictionary<string, object>? properties = null)
        {
            if (!await IsEnabledAsync())
                return;

            await TrackEventAsync(action, TelemetryEventCategories.User, userId, properties);
        }

        public async Task TrackPerformanceMetricAsync(string metricName, double value, string? userId = null, Dictionary<string, object>? properties = null)
        {
            if (!await IsEnabledAsync())
                return;

            var metrics = new Dictionary<string, object> { [metricName] = value };
            await TrackEventAsync("PerformanceMetric", TelemetryEventCategories.Performance, userId, properties, metrics);
        }

        public List<TelemetryEvent> GetQueuedEvents()
        {
            lock (_queueLock)
            {
                var events = _eventQueue.ToList();
                _eventQueue.Clear();
                return events;
            }
        }

        public List<TelemetryEvent> GetEventsByRequestId(string requestId)
        {
            lock (_queueLock)
            {
                return _eventQueue.Where(e => e.RequestId == requestId).ToList();
            }
        }

        public int GetQueueSize()
        {
            lock (_queueLock)
            {
                return _eventQueue.Count;
            }
        }

        private void EnqueueEvent(TelemetryEvent telemetryEvent)
        {
            lock (_queueLock)
            {
                _eventQueue.Enqueue(telemetryEvent);
                
                // Log queue status periodically with correlation info
                if (_eventQueue.Count % 10 == 0)
                {
                    _logger.LogDebug(
                        "Telemetry queue status - QueueSize: {QueueSize}, RequestId: {RequestId}, EventName: {EventName}",
                        _eventQueue.Count,
                        telemetryEvent.RequestId,
                        telemetryEvent.EventName
                    );
                }
            }
        }

        private static string? GetSessionId(HttpContext? httpContext)
        {
            try
            {
                return httpContext?.Session?.Id;
            }
            catch (InvalidOperationException)
            {
                // Session is not configured, return null
                return null;
            }
        }
    }
} 