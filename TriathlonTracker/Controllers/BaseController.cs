using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TriathlonTracker.Models;
using TriathlonTracker.Services;
using Microsoft.Extensions.Logging;

namespace TriathlonTracker.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IAuditService _auditService;
        protected readonly ILogger _logger;
        protected readonly UserManager<User>? _userManager;

        protected BaseController(IAuditService auditService, ILogger logger, UserManager<User>? userManager = null)
        {
            _auditService = auditService;
            _logger = logger;
            _userManager = userManager;
        }

        protected string GetCurrentUserId()
        {
            if (_userManager != null)
                return _userManager.GetUserId(User) ?? "Unknown";
            return User.Identity?.Name ?? "Unknown";
        }

        protected string GetRemoteIp() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        protected string GetUserAgent() => Request.Headers["User-Agent"].ToString();

        protected async Task AuditAsync(string action, string entityType, string? entityId, string details, string? userId, string logLevel)
        {
            await _auditService.LogAsync(action, entityType, entityId, details, userId, GetRemoteIp(), GetUserAgent(), logLevel);
        }

        protected IActionResult ErrorView(string? requestId = null)
        {
            return View("Error", new ErrorViewModel { RequestId = requestId });
        }
    }
} 