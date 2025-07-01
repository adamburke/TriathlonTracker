using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriathlonTracker.Services;

namespace TriathlonTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class TelemetryController : ControllerBase
    {
        private readonly ITelemetryService _telemetryService;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(
            ITelemetryService telemetryService,
            ILogger<TelemetryController> logger)
        {
            _telemetryService = telemetryService;
            _logger = logger;
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetHealth()
        {
            var isEnabled = await _telemetryService.IsEnabledAsync();
            var queueSize = (_telemetryService as TelemetryService)?.GetQueueSize() ?? 0;

            return Ok(new
            {
                IsEnabled = isEnabled,
                QueueSize = queueSize,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("queue")]
        public IActionResult GetQueueInfo()
        {
            var telemetryService = _telemetryService as TelemetryService;
            if (telemetryService == null)
            {
                return BadRequest("Telemetry service not available");
            }

            var queueSize = telemetryService.GetQueueSize();
            var queuedEvents = telemetryService.GetQueuedEvents();

            return Ok(new
            {
                QueueSize = queueSize,
                QueuedEvents = queuedEvents.Select(e => new
                {
                    e.Id,
                    e.EventName,
                    e.EventCategory,
                    e.Timestamp,
                    e.UserId,
                    e.Endpoint,
                    e.StatusCode,
                    e.ResponseTimeMs
                })
            });
        }

        [HttpPost("flush")]
        public IActionResult FlushQueue()
        {
            var telemetryService = _telemetryService as TelemetryService;
            if (telemetryService == null)
            {
                return BadRequest("Telemetry service not available");
            }

            var events = telemetryService.GetQueuedEvents();
            _logger.LogInformation("Flushed {Count} telemetry events from queue", events.Count);

            return Ok(new
            {
                FlushedCount = events.Count,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestTelemetry()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Test different types of telemetry events
            await _telemetryService.TrackEventAsync("TestEvent", "Test", userId, 
                new Dictionary<string, object> { ["testProperty"] = "testValue" });

            await _telemetryService.TrackUserActionAsync("TestAction", userId);

            await _telemetryService.TrackPerformanceMetricAsync("TestMetric", 42.5, userId);

            return Ok(new
            {
                Message = "Test telemetry events queued",
                UserId = userId,
                RequestId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("correlation/{requestId}")]
        public IActionResult GetEventsByRequestId(string requestId)
        {
            var telemetryService = _telemetryService as TelemetryService;
            if (telemetryService == null)
            {
                return BadRequest("Telemetry service not available");
            }

            var events = telemetryService.GetEventsByRequestId(requestId);

            _logger.LogInformation(
                "Retrieved {Count} telemetry events for RequestId: {RequestId}",
                events.Count,
                requestId
            );

            return Ok(new
            {
                RequestId = requestId,
                EventCount = events.Count,
                Events = events.Select(e => new
                {
                    e.Id,
                    e.EventName,
                    e.EventCategory,
                    e.Timestamp,
                    e.UserId,
                    e.Endpoint,
                    e.HttpMethod,
                    e.StatusCode,
                    e.ResponseTimeMs,
                    e.ErrorMessage,
                    e.ErrorType,
                    e.Properties,
                    e.Metrics
                }),
                RetrievedAt = DateTime.UtcNow
            });
        }

        [HttpGet("correlation/current")]
        public IActionResult GetCurrentRequestEvents()
        {
            var requestId = HttpContext.TraceIdentifier;
            return GetEventsByRequestId(requestId);
        }

        [HttpGet("correlation/summary")]
        public IActionResult GetCorrelationSummary()
        {
            var telemetryService = _telemetryService as TelemetryService;
            if (telemetryService == null)
            {
                return BadRequest("Telemetry service not available");
            }

            var allEvents = telemetryService.GetQueuedEvents();
            var requestGroups = allEvents
                .GroupBy(e => e.RequestId)
                .Select(g => new
                {
                    RequestId = g.Key,
                    EventCount = g.Count(),
                    Events = g.Select(e => new
                    {
                        e.EventName,
                        e.EventCategory,
                        e.Timestamp,
                        e.Endpoint,
                        e.StatusCode,
                        e.ResponseTimeMs,
                        e.ErrorType
                    }).ToList()
                })
                .OrderByDescending(g => g.Events.Max(e => e.Timestamp))
                .Take(10) // Top 10 most recent requests
                .ToList();

            return Ok(new
            {
                TotalEvents = allEvents.Count,
                UniqueRequests = requestGroups.Count,
                RecentRequests = requestGroups,
                RetrievedAt = DateTime.UtcNow
            });
        }
    }
} 