using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Services;
using System.Diagnostics;

namespace TriathlonTracker.Middleware
{
    public class TelemetryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TelemetryMiddleware> _logger;

        public TelemetryMiddleware(
            RequestDelegate next,
            ILogger<TelemetryMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var telemetryService = context.RequestServices.GetRequiredService<ITelemetryService>();
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;
            var requestId = context.TraceIdentifier;
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Log request start with correlation details
            _logger.LogInformation(
                "Request started - RequestId: {RequestId}, Method: {Method}, Path: {Path}, UserId: {UserId}, IP: {IpAddress}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                userId ?? "anonymous",
                context.Connection?.RemoteIpAddress?.ToString() ?? "unknown"
            );

            try
            {
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                var requestSize = context.Request.ContentLength ?? 0;

                await _next(context);

                stopwatch.Stop();
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);

                var responseSize = memoryStream.Length;

                // Log request completion with correlation details
                _logger.LogInformation(
                    "Request completed - RequestId: {RequestId}, StatusCode: {StatusCode}, Duration: {Duration}ms, ResponseSize: {ResponseSize} bytes",
                    requestId,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    responseSize
                );

                // Track API request telemetry
                await telemetryService.TrackApiRequestAsync(
                    endpoint: $"{context.Request.Path}{context.Request.QueryString}",
                    httpMethod: context.Request.Method,
                    statusCode: context.Response.StatusCode,
                    responseTimeMs: stopwatch.ElapsedMilliseconds,
                    requestSize: requestSize,
                    responseSize: responseSize,
                    userId: userId
                );
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Log exception with correlation details
                _logger.LogError(
                    ex,
                    "Request failed - RequestId: {RequestId}, Method: {Method}, Path: {Path}, Duration: {Duration}ms, UserId: {UserId}",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    userId ?? "anonymous"
                );

                // Track exception telemetry
                await telemetryService.TrackExceptionAsync(ex, userId);

                // Restore original body stream
                context.Response.Body = originalBodyStream;

                // Re-throw the exception
                throw;
            }
            finally
            {
                // Ensure original body stream is restored
                if (context.Response.Body != originalBodyStream)
                {
                    context.Response.Body = originalBodyStream;
                }
            }
        }
    }
} 