using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TriathlonTracker.Models;
using TriathlonTracker.Services;

namespace TriathlonTracker.Controllers
{
    [Authorize]
    public class ConsentController : Controller
    {
        private readonly IConsentService _consentService;
        private readonly IGdprService _gdprService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ConsentController> _logger;

        public ConsentController(
            IConsentService consentService,
            IGdprService gdprService,
            UserManager<User> userManager,
            ILogger<ConsentController> logger)
        {
            _consentService = consentService;
            _gdprService = gdprService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Consent
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var consentSummary = await _consentService.GetConsentSummaryAsync(userId);
            var consentHistory = await _consentService.GetConsentHistoryAsync(userId);

            var viewModel = new ConsentManagementViewModel
            {
                ConsentSummary = consentSummary,
                ConsentHistory = consentHistory,
                HasDataProcessingConsent = await _consentService.HasDataProcessingConsentAsync(userId),
                HasMarketingConsent = await _consentService.HasMarketingConsentAsync(userId),
                HasAnalyticsConsent = await _consentService.HasAnalyticsConsentAsync(userId)
            };

            return View(viewModel);
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
                    return Json(new { success = true, message = "All consents granted successfully" });
                }

                return Json(new { success = false, message = "Some consents failed to be granted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting all consents for user {UserId}", userId);
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
                    return Json(new { success = true, message = "Selected consents granted successfully" });
                }

                return Json(new { success = false, message = "Some consents failed to be granted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting selected consents for user {UserId}", userId);
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