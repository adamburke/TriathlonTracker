using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TriathlonTracker.Models;
using TriathlonTracker.Services;
using Microsoft.Extensions.Logging;

namespace TriathlonTracker.Controllers
{
    [Authorize]
    public class ConsentController : Controller
    {
        private readonly IConsentService _consentService;
        private readonly IGdprService _gdprService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ConsentController> _logger;
        private readonly IAuditService _auditService;

        public ConsentController(
            IConsentService consentService,
            IGdprService gdprService,
            UserManager<User> userManager,
            ILogger<ConsentController> logger,
            IAuditService auditService)
        {
            _consentService = consentService;
            _gdprService = gdprService;
            _userManager = userManager;
            _logger = logger;
            _auditService = auditService;
        }

        // GET: /Consent
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var consentSummary = await _consentService.GetConsentSummaryAsync(userId);
                var consentHistory = await _consentService.GetConsentHistoryAsync(userId);
                
                var viewModel = new ConsentManagementViewModel
                {
                    ConsentSummary = consentSummary,
                    ConsentHistory = consentHistory.ToList(),
                    HasDataProcessingConsent = await _consentService.HasDataProcessingConsentAsync(userId),
                    HasMarketingConsent = await _consentService.HasMarketingConsentAsync(userId),
                    HasAnalyticsConsent = await _consentService.HasAnalyticsConsentAsync(userId)
                };

                _logger.LogDebug("User {UserId} accessed consent management page", userId);
                await _auditService.LogAsync("ViewConsentManagement", "Consent", userId, "Accessed consent management page", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading consent management for user {UserId}", userId);
                await _auditService.LogAsync("ViewConsentManagementError", "Consent", userId, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }

        // POST: /Consent/Grant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grant(string consentType, string purpose, string legalBasis = "Consent")
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Json(new { success = false, message = "User not authenticated" });

            if (string.IsNullOrEmpty(consentType) || string.IsNullOrEmpty(purpose))
                return Json(new { success = false, message = "Invalid consent parameters" });

            var success = await _consentService.GrantConsentAsync(userId, consentType, purpose, legalBasis);
            
            if (success)
            {
                _logger.LogInformation("Consent granted for user {UserId}, type {ConsentType}", userId, consentType);
                await _auditService.LogAsync("GrantConsent", "Consent", userId, $"Granted consent: {consentType}, purpose: {purpose}, legal basis: {legalBasis}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                return Json(new { success = true, message = "Consent granted successfully" });
            }

            return Json(new { success = false, message = "Failed to grant consent" });
        }

        // POST: /Consent/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(string consentType)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Json(new { success = false, message = "User not authenticated" });

            if (string.IsNullOrEmpty(consentType))
                return Json(new { success = false, message = "Invalid consent type" });

            var success = await _consentService.WithdrawConsentAsync(userId, consentType);
            
            if (success)
            {
                _logger.LogInformation("Consent withdrawn for user {UserId}, type {ConsentType}", userId, consentType);
                await _auditService.LogAsync("WithdrawConsent", "Consent", userId, $"Withdrawn consent: {consentType}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                return Json(new { success = true, message = "Consent withdrawn successfully" });
            }

            return Json(new { success = false, message = "Failed to withdraw consent" });
        }

        // POST: /Consent/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string consentType, bool isGranted)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Json(new { success = false, message = "User not authenticated" });

            var success = await _consentService.UpdateConsentAsync(userId, consentType, isGranted);
            
            if (success)
            {
                var action = isGranted ? "granted" : "withdrawn";
                _logger.LogInformation("Consent {Action} for user {UserId}, type {ConsentType}", action, userId, consentType);
                await _auditService.LogAsync("UpdateConsent", "Consent", userId, $"Updated consent: {consentType} = {isGranted}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                return Json(new { success = true, message = $"Consent {action} successfully" });
            }

            return Json(new { success = false, message = "Failed to update consent" });
        }

        // GET: /Consent/History
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var consentHistory = await _consentService.GetConsentHistoryAsync(userId);
            return View(consentHistory);
        }

        // GET: /Consent/Banner (for new users)
        [AllowAnonymous]
        public IActionResult Banner()
        {
            return PartialView("_ConsentBanner");
        }

        // POST: /Consent/AcceptAll (for consent banner)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptAll()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Json(new { success = false, message = "User not authenticated" });

            try
            {
                // Grant all essential consents
                var consents = new[]
                {
                    new { Type = "DataProcessing", Purpose = "Essential application functionality", LegalBasis = "Contract" },
                    new { Type = "Essential", Purpose = "Essential cookies and functionality", LegalBasis = "LegitimateInterest" }
                };

                var allSuccess = true;
                foreach (var consent in consents)
                {
                    var success = await _consentService.GrantConsentAsync(userId, consent.Type, consent.Purpose, consent.LegalBasis);
                    if (!success)
                        allSuccess = false;
                }

                if (allSuccess)
                {
                    await _auditService.LogAsync("GrantAllConsent", "Consent", userId, "Granted all consent", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                    return Json(new { success = true, message = "All consents granted successfully" });
                }

                return Json(new { success = false, message = "Some consents failed to be granted" });
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "Error accepting all consents for user {UserId}", currentUserId);
                await _auditService.LogAsync("GrantAllConsentError", "Consent", currentUserId, $"Error: {ex.Message}", currentUserId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return Json(new { success = false, message = "An error occurred while processing consents" });
            }
        }

        // POST: /Consent/AcceptSelected (for consent banner)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptSelected([FromBody] ConsentSelectionModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Json(new { success = false, message = "User not authenticated" });

            try
            {
                var allSuccess = true;

                // Always grant essential consent
                var essentialSuccess = await _consentService.GrantConsentAsync(userId, "Essential", "Essential cookies and functionality", "LegitimateInterest");
                if (!essentialSuccess)
                    allSuccess = false;

                // Grant data processing consent if selected
                if (model.DataProcessing)
                {
                    var dataSuccess = await _consentService.GrantConsentAsync(userId, "DataProcessing", "Processing personal data for application functionality", "Contract");
                    if (!dataSuccess)
                        allSuccess = false;
                }

                // Grant marketing consent if selected
                if (model.Marketing)
                {
                    var marketingSuccess = await _consentService.GrantConsentAsync(userId, "Marketing", "Marketing communications and promotional content", "Consent");
                    if (!marketingSuccess)
                        allSuccess = false;
                }

                // Grant analytics consent if selected
                if (model.Analytics)
                {
                    var analyticsSuccess = await _consentService.GrantConsentAsync(userId, "Analytics", "Analytics and performance tracking", "Consent");
                    if (!analyticsSuccess)
                        allSuccess = false;
                }

                if (allSuccess)
                {
                    await _auditService.LogAsync("GrantSelectedConsent", "Consent", userId, $"Granted selected consent: DataProcessing={model.DataProcessing}, Marketing={model.Marketing}, Analytics={model.Analytics}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                    return Json(new { success = true, message = "Selected consents granted successfully" });
                }

                return Json(new { success = false, message = "Some consents failed to be granted" });
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "Error accepting selected consents for user {UserId}", currentUserId);
                await _auditService.LogAsync("GrantSelectedConsentError", "Consent", currentUserId, $"Error: {ex.Message}", currentUserId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return Json(new { success = false, message = "An error occurred while processing consents" });
            }
        }

        // GET: /Consent/Status (AJAX endpoint)
        public async Task<IActionResult> Status()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Json(new { authenticated = false });

            var status = new
            {
                authenticated = true,
                hasDataProcessingConsent = await _consentService.HasDataProcessingConsentAsync(userId),
                hasMarketingConsent = await _consentService.HasMarketingConsentAsync(userId),
                hasAnalyticsConsent = await _consentService.HasAnalyticsConsentAsync(userId)
            };

            return Json(status);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateConsent(string consentType, bool isGranted, string? reason = null)
        {
            try
            {
                var userId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogDebug("User {UserId} updating consent: {ConsentType} = {IsGranted}", userId, consentType, isGranted);
                
                var success = await _consentService.UpdateConsentAsync(userId, consentType, isGranted, reason);
                
                if (success)
                {
                    _logger.LogInformation("User {UserId} successfully updated consent: {ConsentType} = {IsGranted}", userId, consentType, isGranted);
                    await _auditService.LogAsync("UpdateConsent", "Consent", userId, $"Updated consent: {consentType} = {isGranted}, reason: {reason ?? "none"}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                    TempData["Success"] = "Consent updated successfully.";
                }
                else
                {
                    _logger.LogWarning("User {UserId} failed to update consent: {ConsentType}", userId, consentType);
                    await _auditService.LogAsync("UpdateConsentFailed", "Consent", userId, $"Failed to update consent: {consentType}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                    TempData["Error"] = "Failed to update consent.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "Error updating consent for user {UserId}: {ConsentType}", currentUserId, consentType);
                await _auditService.LogAsync("UpdateConsentError", "Consent", currentUserId, $"Error updating {consentType}: {ex.Message}", currentUserId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                TempData["Error"] = "An error occurred while updating consent.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> RevokeAllConsent()
        {
            try
            {
                var userId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogDebug("User {UserId} revoking all consent", userId);
                
                var consentTypes = new[] { "DataProcessing", "Marketing", "Analytics" };
                var success = await _consentService.BulkWithdrawConsentAsync(userId, consentTypes);
                
                if (success)
                {
                    _logger.LogInformation("User {UserId} successfully revoked all consent", userId);
                    await _auditService.LogAsync("RevokeAllConsent", "Consent", userId, "Revoked all consent", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                    TempData["Success"] = "All consent has been revoked.";
                }
                else
                {
                    _logger.LogWarning("User {UserId} failed to revoke all consent", userId);
                    await _auditService.LogAsync("RevokeAllConsentFailed", "Consent", userId, "Failed to revoke all consent", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                    TempData["Error"] = "Failed to revoke all consent.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "Error revoking all consent for user {UserId}", currentUserId);
                await _auditService.LogAsync("RevokeAllConsentError", "Consent", currentUserId, $"Error: {ex.Message}", currentUserId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                TempData["Error"] = "An error occurred while revoking consent.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantAllConsent()
        {
            try
            {
                var userId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogDebug("User {UserId} granting all consent", userId);
                
                var dataSuccess = await _consentService.GrantConsentAsync(userId, "DataProcessing", "Processing personal data for application functionality", "Contract");
                var marketingSuccess = await _consentService.GrantConsentAsync(userId, "Marketing", "Marketing communications and promotional content", "Consent");
                var analyticsSuccess = await _consentService.GrantConsentAsync(userId, "Analytics", "Analytics and performance tracking", "Consent");
                
                if (dataSuccess && marketingSuccess && analyticsSuccess)
                {
                    _logger.LogInformation("User {UserId} successfully granted all consent", userId);
                    await _auditService.LogAsync("GrantAllConsent", "Consent", userId, "Granted all consent", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                    TempData["Success"] = "All consent has been granted.";
                }
                else
                {
                    _logger.LogWarning("User {UserId} failed to grant all consent", userId);
                    await _auditService.LogAsync("GrantAllConsentFailed", "Consent", userId, "Failed to grant all consent", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                    TempData["Error"] = "Failed to grant all consent.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "Error granting all consent for user {UserId}", currentUserId);
                await _auditService.LogAsync("GrantAllConsentError", "Consent", currentUserId, $"Error: {ex.Message}", currentUserId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                TempData["Error"] = "An error occurred while granting consent.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportConsentData()
        {
            try
            {
                var userId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogDebug("User {UserId} exporting consent data", userId);
                
                var consentHistory = await _consentService.GetConsentHistoryAsync(userId);
                var consentSummary = await _consentService.GetConsentSummaryAsync(userId);
                
                var exportData = new
                {
                    UserId = userId,
                    ExportDate = DateTime.UtcNow,
                    ConsentSummary = consentSummary,
                    ConsentHistory = consentHistory.Select(c => new
                    {
                        c.ConsentType,
                        c.IsGranted,
                        c.ConsentDate,
                        c.WithdrawnDate,
                        c.Purpose,
                        c.LegalBasis,
                        c.ConsentVersion
                    })
                };

                _logger.LogInformation("User {UserId} successfully exported consent data", userId);
                await _auditService.LogAsync("ExportConsentData", "Consent", userId, "Exported consent data", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");

                return Json(exportData);
            }
            catch (Exception ex)
            {
                var userId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "Error exporting consent data for user {UserId}", userId);
                await _auditService.LogAsync("ExportConsentDataError", "Consent", userId, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return Json(new { error = "Failed to export consent data" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConsentHistory()
        {
            try
            {
                var userId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogDebug("User {UserId} viewing consent history", userId);
                
                var consentHistory = await _consentService.GetConsentHistoryAsync(userId);
                
                _logger.LogInformation("User {UserId} viewed consent history with {Count} records", userId, consentHistory.Count());
                await _auditService.LogAsync("ViewConsentHistory", "Consent", userId, $"Viewed consent history with {consentHistory.Count()} records", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                
                return View(consentHistory);
            }
            catch (Exception ex)
            {
                var userId = _userManager.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "Error loading consent history for user {UserId}", userId);
                await _auditService.LogAsync("ViewConsentHistoryError", "Consent", userId, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
                return View("Error");
            }
        }
    }

    // View Models
    public class ConsentManagementViewModel
    {
        public object ConsentSummary { get; set; } = new();
        public IEnumerable<ConsentRecord> ConsentHistory { get; set; } = new List<ConsentRecord>();
        public bool HasDataProcessingConsent { get; set; }
        public bool HasMarketingConsent { get; set; }
        public bool HasAnalyticsConsent { get; set; }
    }

    public class ConsentSelectionModel
    {
        public bool DataProcessing { get; set; }
        public bool Marketing { get; set; }
        public bool Analytics { get; set; }
    }
}