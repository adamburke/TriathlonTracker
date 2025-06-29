using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriathlonTracker.Models;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Services;
using Microsoft.AspNetCore.Identity;

namespace TriathlonTracker.Controllers;

[Authorize]
public class HomeController : BaseController
{
    public HomeController(ILogger<HomeController> logger, IAuditService auditService, UserManager<User>? userManager = null)
        : base(auditService, logger, userManager) { }

    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogDebug("Home page accessed by {UserId}", userId);
            await AuditAsync("ViewHomePage", "Home", null, "Home page accessed", userId, "Information");
            if (User?.IsInRole("Admin") == true)
                return RedirectToAction("Dashboard", "Admin");
            return RedirectToAction("Index", "Triathlon");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accessing home page");
            await AuditAsync("ViewHomePageError", "Home", null, $"Error: {ex.Message}", "System", "Error");
            return ErrorView();
        }
    }

    public async Task<IActionResult> Privacy()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogDebug("Privacy page accessed by {UserId}", userId);
            await AuditAsync("ViewPrivacyPage", "Home", null, "Privacy page accessed", userId, "Information");
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accessing privacy page");
            await AuditAsync("ViewPrivacyPageError", "Home", null, $"Error: {ex.Message}", "System", "Error");
            return ErrorView();
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Error()
    {
        var userId = GetCurrentUserId();
        _logger.LogWarning("Error page accessed by {UserId}", userId);
        await AuditAsync("ViewErrorPage", "Home", null, "Error page accessed", userId, "Warning");
        return ErrorView();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> NotFoundPage()
    {
        var userId = GetCurrentUserId();
        var requestedPath = HttpContext.Request.Path;
        _logger.LogWarning("404 Not Found - Path: {Path}, User: {UserId}", requestedPath, userId);
        await AuditAsync("ViewNotFoundPage", "Home", null, $"404 Not Found - Path: {requestedPath}", userId, "Warning");
        Response.StatusCode = 404;
        return View("NotFound");
    }
}
