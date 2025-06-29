using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriathlonTracker.Models;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Services;

namespace TriathlonTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAuditService _auditService;

    public HomeController(ILogger<HomeController> logger, IAuditService auditService)
    {
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
            _logger.LogDebug("Home page accessed by {UserId}", userId);
            
            await _auditService.LogAsync("ViewHomePage", "Home", null, "Home page accessed", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
            
            // Redirect admin users to admin dashboard, regular users to triathlon section
            if (User?.IsInRole("Admin") == true)
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return RedirectToAction("Index", "Triathlon");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accessing home page");
            await _auditService.LogAsync("ViewHomePageError", "Home", null, $"Error: {ex.Message}", "System", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
            return View("Error");
        }
    }

    public async Task<IActionResult> Privacy()
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
            _logger.LogDebug("Privacy page accessed by {UserId}", userId);
            
            await _auditService.LogAsync("ViewPrivacyPage", "Home", null, "Privacy page accessed", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accessing privacy page");
            await _auditService.LogAsync("ViewPrivacyPageError", "Home", null, $"Error: {ex.Message}", "System", HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
            return View("Error");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Error()
    {
        var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
        _logger.LogWarning("Error page accessed by {UserId}", userId);
        
        await _auditService.LogAsync("ViewErrorPage", "Home", null, "Error page accessed", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
        
        return View();
    }
}
