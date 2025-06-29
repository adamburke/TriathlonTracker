using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TriathlonTracker.Data;
using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogAsync(string action, string entityType, string? entityId, string? details, string? userId, string? ipAddress, string? userAgent, string logLevel = "Information")
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
                LogLevel = logLevel
            };
            await LogAsync(log);
        }
    }
} 