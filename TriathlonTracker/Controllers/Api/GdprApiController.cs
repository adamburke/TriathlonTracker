using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TriathlonTracker.Models;
using TriathlonTracker.Services;

namespace TriathlonTracker.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("GdprApiPolicy")]
    public class GdprApiController : ControllerBase
    {
        private readonly IEnhancedGdprService _enhancedGdprService;
        private readonly IGdprService _gdprService;
        private readonly IConsentService _consentService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<GdprApiController> _logger;

        public GdprApiController(
            IEnhancedGdprService enhancedGdprService,
            IGdprService gdprService,
            IConsentService consentService,
            UserManager<User> userManager,
            ILogger<GdprApiController> logger)
        {
            _enhancedGdprService = enhancedGdprService;
            _gdprService = gdprService;
            _consentService = consentService;
            _userManager = userManager;
            _logger = logger;
        }

        #region Data Export API

        /// <summary>
        /// Request data export for the authenticated user
        /// </summary>
        [HttpPost("export/request")]
        public async Task<IActionResult> RequestDataExport([FromBody] DataExportRequestModel request)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var exportRequest = await _enhancedGdprService.CreateDataExportRequestAsync(userId, request.Format);
                
                // Process asynchronously
                _ = Task.Run(async () => await _enhancedGdprService.ProcessDataExportRequestAsync(exportRequest.Id));

                return Ok(new
                {
                    requestId = exportRequest.Id,
                    status = exportRequest.Status,
                    format = exportRequest.Format,
                    requestDate = exportRequest.RequestDate,
                    message = "Data export request submitted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error requesting data export for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get data export requests for the authenticated user
        /// </summary>
        [HttpGet("export/requests")]
        public async Task<IActionResult> GetDataExportRequests()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var requests = await _enhancedGdprService.GetUserExportRequestsAsync(userId);
                
                var response = requests.Select(r => new
                {
                    id = r.Id,
                    format = r.Format,
                    status = r.Status,
                    requestDate = r.RequestDate,
                    completedDate = r.CompletedDate,
                    expirationDate = r.ExpirationDate,
                    downloadUrl = r.DownloadUrl,
                    fileName = r.FileName,
                    fileSizeBytes = r.FileSizeBytes,
                    downloadCount = r.DownloadCount,
                    lastDownloadDate = r.LastDownloadDate
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting data export requests for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get data export request status
        /// </summary>
        [HttpGet("export/requests/{requestId}/status")]
        public async Task<IActionResult> GetExportRequestStatus(int requestId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var requests = await _enhancedGdprService.GetUserExportRequestsAsync(userId);
                var request = requests.FirstOrDefault(r => r.Id == requestId);

                if (request == null)
                    return NotFound(new { error = "Export request not found" });

                return Ok(new
                {
                    id = request.Id,
                    status = request.Status,
                    requestDate = request.RequestDate,
                    completedDate = request.CompletedDate,
                    expirationDate = request.ExpirationDate,
                    errorMessage = request.ErrorMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting export request status for user {UserId}, request {RequestId}", userId, requestId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        #endregion

        #region Data Rectification API

        /// <summary>
        /// Request data rectification
        /// </summary>
        [HttpPost("rectification/request")]
        public async Task<IActionResult> RequestDataRectification([FromBody] DataRectificationRequestModel request)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var rectificationRequest = await _enhancedGdprService.CreateRectificationRequestAsync(
                    userId, request.DataType, request.FieldName, request.CurrentValue, request.RequestedValue, request.Reason);

                return Ok(new
                {
                    requestId = rectificationRequest.Id,
                    status = rectificationRequest.Status,
                    requestDate = rectificationRequest.RequestDate,
                    priority = rectificationRequest.Priority,
                    message = "Data rectification request submitted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error requesting data rectification for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get data rectification requests for the authenticated user
        /// </summary>
        [HttpGet("rectification/requests")]
        public async Task<IActionResult> GetDataRectificationRequests()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var requests = await _enhancedGdprService.GetUserRectificationRequestsAsync(userId);
                
                var response = requests.Select(r => new
                {
                    id = r.Id,
                    dataType = r.DataType,
                    fieldName = r.FieldName,
                    currentValue = r.CurrentValue,
                    requestedValue = r.RequestedValue,
                    reason = r.Reason,
                    status = r.Status,
                    requestDate = r.RequestDate,
                    reviewDate = r.ReviewDate,
                    completedDate = r.CompletedDate,
                    reviewNotes = r.ReviewNotes,
                    rejectionReason = r.RejectionReason,
                    priority = r.Priority
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting data rectification requests for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        #endregion

        #region Account Deletion API

        /// <summary>
        /// Request account deletion
        /// </summary>
        [HttpPost("deletion/request")]
        public async Task<IActionResult> RequestAccountDeletion([FromBody] AccountDeletionRequestModel request)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var deletionRequest = await _enhancedGdprService.CreateAccountDeletionRequestAsync(
                    userId, request.Reason, request.DeletionType);

                return Ok(new
                {
                    requestId = deletionRequest.Id,
                    status = deletionRequest.Status,
                    deletionType = deletionRequest.DeletionType,
                    requestDate = deletionRequest.RequestDate,
                    confirmationToken = deletionRequest.ConfirmationToken,
                    tokenExpirationDate = deletionRequest.TokenExpirationDate,
                    recoveryPeriodDays = deletionRequest.RecoveryPeriodDays,
                    message = "Account deletion request submitted. Please check your email for confirmation instructions."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error requesting account deletion for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Recover account from deletion
        /// </summary>
        [HttpPost("deletion/recover")]
        public async Task<IActionResult> RecoverAccount()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var success = await _enhancedGdprService.RecoverAccountAsync(userId);
                
                if (success)
                {
                    return Ok(new { message = "Account recovery successful. Your deletion request has been cancelled." });
                }
                else
                {
                    return BadRequest(new { error = "Account recovery failed. The recovery period may have expired." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error recovering account for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        #endregion

        #region Consent Management API

        /// <summary>
        /// Get consent history for the authenticated user
        /// </summary>
        [HttpGet("consent/history")]
        public async Task<IActionResult> GetConsentHistory()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var consents = await _consentService.GetConsentHistoryAsync(userId);
                
                var response = consents.Select(c => new
                {
                    id = c.Id,
                    consentType = c.ConsentType,
                    isGranted = c.IsGranted,
                    consentDate = c.ConsentDate,
                    withdrawnDate = c.WithdrawnDate,
                    purpose = c.Purpose,
                    legalBasis = c.LegalBasis,
                    consentVersion = c.ConsentVersion
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting consent history for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Grant consent
        /// </summary>
        [HttpPost("consent/grant")]
        public async Task<IActionResult> GrantConsent([FromBody] ConsentRequestModel request)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var success = await _consentService.GrantConsentAsync(
                    userId, request.ConsentType, request.Purpose, request.LegalBasis, request.ConsentVersion);

                if (success)
                {
                    return Ok(new { message = "Consent granted successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to grant consent" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error granting consent for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Withdraw consent
        /// </summary>
        [HttpPost("consent/withdraw")]
        public async Task<IActionResult> WithdrawConsent([FromBody] ConsentWithdrawModel request)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var success = await _consentService.WithdrawConsentAsync(userId, request.ConsentType);

                if (success)
                {
                    return Ok(new { message = "Consent withdrawn successfully" });
                }
                else
                {
                    return BadRequest(new { error = "Failed to withdraw consent" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error withdrawing consent for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        #endregion

        #region Data Processing Logs API

        /// <summary>
        /// Get data processing logs for the authenticated user
        /// </summary>
        [HttpGet("processing-logs")]
        public async Task<IActionResult> GetProcessingLogs([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                fromDate ??= DateTime.UtcNow.AddDays(-30);
                toDate ??= DateTime.UtcNow;

                var logs = await _gdprService.GetDataProcessingLogsAsync(userId, fromDate, toDate);
                
                var response = logs.Select(l => new
                {
                    id = l.Id,
                    action = l.Action,
                    dataType = l.DataType,
                    description = l.Description,
                    purpose = l.Purpose,
                    legalBasis = l.LegalBasis,
                    processedAt = l.ProcessedAt,
                    processedBy = l.ProcessedBy,
                    isAutomated = l.IsAutomated
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting processing logs for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        #endregion

        #region Privacy Dashboard API

        /// <summary>
        /// Get enhanced privacy dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetPrivacyDashboard()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var dashboardData = await _enhancedGdprService.GetEnhancedPrivacyDashboardAsync(userId);
                var dataUsage = await _enhancedGdprService.GetDataUsageAnalyticsAsync(userId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
                var retentionTimeline = await _enhancedGdprService.GetDataRetentionTimelineAsync(userId);

                return Ok(new
                {
                    dashboard = dashboardData,
                    dataUsage = dataUsage,
                    retentionTimeline = retentionTimeline
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Error getting privacy dashboard for user {UserId}", userId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        #endregion
    }

    #region API Models

    public class DataExportRequestModel
    {
        public string Format { get; set; } = "JSON";
    }

    public class DataRectificationRequestModel
    {
        public string DataType { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string RequestedValue { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class AccountDeletionRequestModel
    {
        public string Reason { get; set; } = string.Empty;
        public string DeletionType { get; set; } = "SoftDelete";
    }

    public class ConsentRequestModel
    {
        public string ConsentType { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string LegalBasis { get; set; } = "Consent";
        public string ConsentVersion { get; set; } = "1.0";
    }

    public class ConsentWithdrawModel
    {
        public string ConsentType { get; set; } = string.Empty;
    }

    #endregion
}