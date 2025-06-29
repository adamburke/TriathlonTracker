using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Services;

namespace TriathlonTracker.Controllers
{
    public class PrivacyController : Controller
    {
        private readonly ILogger<PrivacyController> _logger;
        private readonly IAuditService _auditService;

        public PrivacyController(ILogger<PrivacyController> logger, IAuditService auditService)
        {
            _logger = logger;
            _auditService = auditService;
        }

        [HttpGet]
        public IActionResult Dashboard(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
            return View();
        }

        [HttpGet]
        public IActionResult Consent(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
            return View();
        }

        [HttpGet]
        public IActionResult Policy(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
            return View();
        }

        [HttpGet]
        public IActionResult Cookie(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
            return View();
        }

        [HttpGet]
        public IActionResult Contact(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }
            return View();
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogDebug("User {UserId} accessing privacy policy page", userId);
                
                await _auditService.LogAsync("ViewPrivacyPolicy", "Privacy", null, "Privacy policy page accessed", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                
                return View();
            }
            catch (Exception ex)
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogError(ex, "Error accessing privacy policy page for user {UserId}", userId);
                await _auditService.LogAsync("ViewPrivacyPolicyError", "Privacy", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
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

        public async Task<IActionResult> Contact()
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogDebug("User {UserId} accessing privacy contact page", userId);
                
                await _auditService.LogAsync("ViewPrivacyContact", "Privacy", null, "Privacy contact page accessed", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                
                return View();
            }
            catch (Exception ex)
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogError(ex, "Error accessing privacy contact page for user {UserId}", userId);
                await _auditService.LogAsync("ViewPrivacyContactError", "Privacy", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }

        public async Task<IActionResult> Cookie()
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogDebug("User {UserId} accessing cookie policy page", userId);
                
                await _auditService.LogAsync("ViewCookiePolicy", "Privacy", null, "Cookie policy page accessed", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                
                return View();
            }
            catch (Exception ex)
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogError(ex, "Error accessing cookie policy page for user {UserId}", userId);
                await _auditService.LogAsync("ViewCookiePolicyError", "Privacy", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
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

        public async Task<IActionResult> Consent()
        {
            try
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogDebug("User {UserId} accessing consent management page", userId);
                
                await _auditService.LogAsync("ViewConsentManagement", "Privacy", null, "Consent management page accessed", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                
                return View();
            }
            catch (Exception ex)
            {
                var userId = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonymous";
                _logger.LogError(ex, "Error accessing consent management page for user {UserId}", userId);
                await _auditService.LogAsync("ViewConsentManagementError", "Privacy", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = User.Identity?.Name ?? "Anonymous";
                if (!User.Identity?.IsAuthenticated == true)
                {
                    _logger.LogWarning("Unauthenticated user attempted to access privacy dashboard");
                    await _auditService.LogAsync("AccessDenied", "Privacy", null, "Unauthenticated user attempted to access privacy dashboard", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogDebug("User {UserId} accessing privacy dashboard", userId);
                
                await _auditService.LogAsync("ViewPrivacyDashboard", "Privacy", null, "Privacy dashboard accessed", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                
                return View();
            }
            catch (Exception ex)
            {
                var userId = User.Identity?.Name ?? "Anonymous";
                _logger.LogError(ex, "Error accessing privacy dashboard for user {UserId}", userId);
                await _auditService.LogAsync("ViewPrivacyDashboardError", "Privacy", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }
    }
}