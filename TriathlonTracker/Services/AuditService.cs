using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Data;
using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(AuditLog log)
        {
            try
            {
                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save audit log for action {Action} on entity {EntityType}", log.Action, log.EntityType);
                // Don't re-throw to prevent cascading failures
            }
        }

        public async Task LogAsync(string action, string entityType, string? entityId, string? details, string? userId, string? ipAddress, string? userAgent, string logLevel = "Information")
        {
            try
            {
                var log = new AuditLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    Details = details,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    LogLevel = logLevel,
                    Timestamp = DateTime.UtcNow
                };
                await LogAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for action {Action} on entity {EntityType}", action, entityType);
                // Don't re-throw to prevent cascading failures
            }
        }
    }
} 