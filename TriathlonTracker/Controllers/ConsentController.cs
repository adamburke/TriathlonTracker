using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TriathlonTracker.Models;
using TriathlonTracker.Services;
using Microsoft.Extensions.Logging;

namespace TriathlonTracker.Controllers
{
    [Authorize]
    public class ConsentController : BaseController
    {
        private readonly IConsentService _consentService;
        private readonly IGdprService _gdprService;

        public ConsentController(
            IConsentService consentService,
            IGdprService gdprService,
            UserManager<User> userManager,
            ILogger<ConsentController> logger,
            IAuditService auditService)
            : base(auditService, logger, userManager)
        {
            _consentService = consentService;
            _gdprService = gdprService;
        }

        // GET: /Consent
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
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
                await AuditAsync("ViewConsentManagement", "Consent", userId, "Accessed consent management page", userId, "Information");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                await AuditAsync("ViewConsentManagementError", "Consent", userId, $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
            }
        }

        // POST: /Consent/Grant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grant(string consentType, string purpose, string legalBasis = "Consent")
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == "Unknown")
                    return Json(new { success = false, message = "User not authenticated" });

                if (string.IsNullOrEmpty(consentType) || string.IsNullOrEmpty(purpose))
                    return Json(new { success = false, message = "Invalid consent parameters" });

                var success = await _consentService.GrantConsentAsync(userId, consentType, purpose, legalBasis);
                
                if (success)
                {
                    await AuditAsync("GrantConsent", "Consent", userId, $"Granted consent: {consentType}, purpose: {purpose}, legal basis: {legalBasis}", userId, "Information");
                    return Json(new { success = true, message = "Consent granted successfully" });
                }

                return Json(new { success = false, message = "Failed to grant consent" });
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                await AuditAsync("GrantConsentException", "Consent", null, $"Exception: {ex.Message}", userId, "Error");
                return Json(new { success = false, message = "An error occurred while granting consent" });
            }
        }

        // POST: /Consent/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(string consentType)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == "Unknown")
                    return Json(new { success = false, message = "User not authenticated" });

                if (string.IsNullOrEmpty(consentType))
                    return Json(new { success = false, message = "Invalid consent type" });

                var success = await _consentService.WithdrawConsentAsync(userId, consentType);
                
                if (success)
                {
                    await AuditAsync("WithdrawConsent", "Consent", userId, $"Withdrawn consent: {consentType}", userId, "Information");
                    return Json(new { success = true, message = "Consent withdrawn successfully" });
                }

                return Json(new { success = false, message = "Failed to withdraw consent" });
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                await AuditAsync("WithdrawConsentException", "Consent", null, $"Exception: {ex.Message}", userId, "Error");
                return Json(new { success = false, message = "An error occurred while withdrawing consent" });
            }
        }

        // POST: /Consent/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string consentType, bool isGranted)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == "Unknown")
                    return Json(new { success = false, message = "User not authenticated" });

                var success = await _consentService.UpdateConsentAsync(userId, consentType, isGranted);
                
                if (success)
                {
                    var action = isGranted ? "granted" : "withdrawn";
                    await AuditAsync("UpdateConsent", "Consent", userId, $"Updated consent: {consentType} = {isGranted}", userId, "Information");
                    return Json(new { success = true, message = $"Consent {action} successfully" });
                }

                return Json(new { success = false, message = "Failed to update consent" });
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                await AuditAsync("UpdateConsentException", "Consent", null, $"Exception: {ex.Message}", userId, "Error");
                return Json(new { success = false, message = "An error occurred while updating consent" });
            }
        }

        // GET: /Consent/History
        public async Task<IActionResult> History()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == "Unknown")
                    return RedirectToAction("Login", "Account");

                var consentHistory = await _consentService.GetConsentHistoryAsync(userId);
                
                await AuditAsync("ViewConsentHistory", "Consent", userId, "Viewed consent history", userId, "Information");
                
                return View(consentHistory);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                await AuditAsync("ViewConsentHistoryException", "Consent", null, $"Exception: {ex.Message}", userId, "Error");
                return ErrorView();
            }
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
            var userId = GetCurrentUserId();
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
                    await AuditAsync("GrantAllConsent", "Consent", userId, "Granted all consent", userId, "Information");
                    return Json(new { success = true, message = "All consents granted successfully" });
                }

                return Json(new { success = false, message = "Some consents failed to be granted" });
            }
            catch (Exception ex)
            {
                var currentUserId = GetCurrentUserId() ?? "Unknown";
                await AuditAsync("GrantAllConsentError", "Consent", null, $"Error: {ex.Message}", currentUserId, "Error");
                return Json(new { success = false, message = "An error occurred while processing consents" });
            }
        }

        // POST: /Consent/AcceptSelected (for consent banner)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptSelected([FromBody] ConsentSelectionModel model)
        {
            var userId = GetCurrentUserId();
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
                    await AuditAsync("GrantSelectedConsent", "Consent", userId, $"Granted selected consent: DataProcessing={model.DataProcessing}, Marketing={model.Marketing}, Analytics={model.Analytics}", userId, "Information");
                    return Json(new { success = true, message = "Selected consents granted successfully" });
                }

                return Json(new { success = false, message = "Some consents failed to be granted" });
            }
            catch (Exception ex)
            {
                var currentUserId = GetCurrentUserId() ?? "Unknown";
                await AuditAsync("GrantSelectedConsentError", "Consent", null, $"Error: {ex.Message}", currentUserId, "Error");
                return Json(new { success = false, message = "An error occurred while processing consents" });
            }
        }

        // GET: /Consent/Status (AJAX endpoint)
        public async Task<IActionResult> Status()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == "Unknown")
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
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                await AuditAsync("GetConsentStatusException", "Consent", null, $"Exception: {ex.Message}", userId, "Error");
                return Json(new { authenticated = false, error = "Failed to get consent status" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateConsent(string consentType, bool isGranted, string? reason = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                await AuditAsync("UpdateConsent", "Consent", userId, $"Updated consent: {consentType} = {isGranted}, reason: {reason ?? "none"}", userId, "Information");
                
                var success = await _consentService.UpdateConsentAsync(userId, consentType, isGranted, reason);
                
                if (success)
                {
                    TempData["Success"] = "Consent updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update consent.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var currentUserId = GetCurrentUserId() ?? "Unknown";
                await AuditAsync("UpdateConsentError", "Consent", null, $"Error updating {consentType}: {ex.Message}", currentUserId, "Error");
                TempData["Error"] = "An error occurred while updating consent.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> RevokeAllConsent()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var consentTypes = new[] { "DataProcessing", "Marketing", "Analytics" };
                var success = await _consentService.BulkWithdrawConsentAsync(userId, consentTypes);
                
                if (success)
                {
                    TempData["Success"] = "All consent has been revoked.";
                }
                else
                {
                    TempData["Error"] = "Failed to revoke all consent.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var currentUserId = GetCurrentUserId() ?? "Unknown";
                await AuditAsync("RevokeAllConsentError", "Consent", null, $"Error: {ex.Message}", currentUserId, "Error");
                TempData["Error"] = "An error occurred while revoking consent.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> GrantAllConsent()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var dataSuccess = await _consentService.GrantConsentAsync(userId, "DataProcessing", "Processing personal data for application functionality", "Contract");
                var marketingSuccess = await _consentService.GrantConsentAsync(userId, "Marketing", "Marketing communications and promotional content", "Consent");
                var analyticsSuccess = await _consentService.GrantConsentAsync(userId, "Analytics", "Analytics and performance tracking", "Consent");
                
                if (dataSuccess && marketingSuccess && analyticsSuccess)
                {
                    TempData["Success"] = "All consent has been granted.";
                }
                else
                {
                    TempData["Error"] = "Failed to grant all consent.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var currentUserId = GetCurrentUserId() ?? "Unknown";
                await AuditAsync("GrantAllConsentError", "Consent", null, $"Error: {ex.Message}", currentUserId, "Error");
                TempData["Error"] = "An error occurred while granting consent.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportConsentData()
        {
            string userId = GetCurrentUserId();
            try
            {
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
                await AuditAsync("ExportConsentData", "Consent", userId, "Exported consent data", userId, "Information");
                return Json(exportData);
            }
            catch (Exception ex)
            {
                await AuditAsync("ExportConsentDataError", "Consent", null, $"Error: {ex.Message}", userId, "Error");
                return Json(new { error = "Failed to export consent data" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConsentHistory()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var consentHistory = await _consentService.GetConsentHistoryAsync(userId);
                
                await AuditAsync("ViewConsentHistory", "Consent", userId, $"Viewed consent history with {consentHistory.Count()} records", userId, "Information");
                
                return View(consentHistory);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                await AuditAsync("ViewConsentHistoryError", "Consent", null, $"Error: {ex.Message}", userId, "Error");
                return ErrorView();
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