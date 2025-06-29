using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Services;
using Microsoft.AspNetCore.Identity;
using TriathlonTracker.Models;

namespace TriathlonTracker.Controllers
{
    public class PrivacyController : BaseController
    {
        public PrivacyController(ILogger<PrivacyController> logger, IAuditService auditService, UserManager<User>? userManager = null)
            : base(auditService, logger, userManager) { }

        private void SetCulture(string? culture)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(string? culture = null)
        {
            SetCulture(culture);
            try
            {
                var userId = GetCurrentUserId();
                if (!User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogWarning("Unauthenticated user attempted to access privacy dashboard");
                    await AuditAsync("AccessDenied", "Privacy", null, "Unauthenticated user attempted to access privacy dashboard", userId, "Warning");
                    return RedirectToAction("Login", "Account");
                }
                _logger.LogDebug("User {UserId} accessing privacy dashboard", userId);
                await AuditAsync("ViewPrivacyDashboard", "Privacy", null, "Privacy dashboard accessed", userId, "Information");
                return View();
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error accessing privacy dashboard for user {UserId}", userId);
                await AuditAsync("ViewPrivacyDashboardError", "Privacy", null, $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Consent(string? culture = null)
        {
            SetCulture(culture);
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} accessing consent management page", userId);
                await AuditAsync("ViewConsentManagement", "Privacy", null, "Consent management page accessed", userId, "Information");
                return View();
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error accessing consent management page for user {UserId}", userId);
                await AuditAsync("ViewConsentManagementError", "Privacy", null, $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Policy(string? culture = null)
        {
            SetCulture(culture);
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} accessing privacy policy page", userId);
                await AuditAsync("ViewPrivacyPolicy", "Privacy", null, "Privacy policy page accessed", userId, "Information");
                return View();
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error accessing privacy policy page for user {UserId}", userId);
                await AuditAsync("ViewPrivacyPolicyError", "Privacy", null, $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Cookie(string? culture = null)
        {
            SetCulture(culture);
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} accessing cookie policy page", userId);
                await AuditAsync("ViewCookiePolicy", "Privacy", null, "Cookie policy page accessed", userId, "Information");
                return View();
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error accessing cookie policy page for user {UserId}", userId);
                await AuditAsync("ViewCookiePolicyError", "Privacy", null, $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Contact(string? culture = null)
        {
            SetCulture(culture);
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} accessing privacy contact page", userId);
                await AuditAsync("ViewPrivacyContact", "Privacy", null, "Privacy contact page accessed", userId, "Information");
                return View();
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error accessing privacy contact page for user {UserId}", userId);
                await AuditAsync("ViewPrivacyContactError", "Privacy", null, $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
            }
        }

        public async Task<IActionResult> Policy()
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogDebug("User {UserId} accessing privacy policy details", userId);
                
                await _auditService.LogAsync("ViewPrivacyPolicyDetails", "Privacy", null, "Privacy policy details accessed", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                
                return View();
            }
            catch (Exception ex)
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogError(ex, "Error accessing privacy policy details for user {UserId}", userId);
                await _auditService.LogAsync("ViewPrivacyPolicyDetailsError", "Privacy", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }

        public async Task<IActionResult> CookiePolicy()
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogDebug("User {UserId} accessing detailed cookie policy", userId);
                
                await _auditService.LogAsync("ViewDetailedCookiePolicy", "Privacy", null, "Detailed cookie policy accessed", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                
                return View();
            }
            catch (Exception ex)
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogError(ex, "Error accessing detailed cookie policy for user {UserId}", userId);
                await _auditService.LogAsync("ViewDetailedCookiePolicyError", "Privacy", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }
    }
}