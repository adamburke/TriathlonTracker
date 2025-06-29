using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TriathlonTracker.Models;
using TriathlonTracker.Services;
using Microsoft.Extensions.Logging;

namespace TriathlonTracker.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("GdprApiPolicy")]
    public class GdprApiController : BaseController
    {
        private readonly IEnhancedGdprService _enhancedGdprService;
        private readonly IGdprService _gdprService;
        private readonly IConsentService _consentService;

        public GdprApiController(
            IEnhancedGdprService enhancedGdprService,
            IGdprService gdprService,
            IConsentService consentService,
            IAuditService auditService,
            ILogger<GdprApiController> logger,
            UserManager<User> userManager)
            : base(auditService, logger, userManager)
        {
            _enhancedGdprService = enhancedGdprService;
            _gdprService = gdprService;
            _consentService = consentService;
        }

        #region Data Export API

        /// <summary>
        /// Request data export for the authenticated user
        /// </summary>
        [HttpPost("export/request")]
        public async Task<IActionResult> RequestDataExport([FromBody] DataExportRequestModel request)
        {
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                _logger.LogDebug("API: User {UserId} requesting data export in format {Format}", userId, request.Format);
                
                var exportRequest = await _enhancedGdprService.CreateDataExportRequestAsync(userId, request.Format);
                
                // Process asynchronously
                _ = Task.Run(async () => await _enhancedGdprService.ProcessDataExportRequestAsync(exportRequest.Id));
                
                _logger.LogInformation("API: User {UserId} successfully requested data export {RequestId}", userId, exportRequest.Id);
                await AuditAsync("ApiRequestDataExport", "GdprApi", exportRequest.Id.ToString(), $"Requested data export in {request.Format} format", userId, "Information");
                
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error requesting data export for user {UserId}", currentUserId);
                await AuditAsync("ApiRequestDataExportError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get data export requests for the authenticated user
        /// </summary>
        [HttpGet("export/requests")]
        public async Task<IActionResult> GetDataExportRequests()
        {
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error getting data export requests for user {UserId}", currentUserId);
                await AuditAsync("ApiGetDataExportRequestsError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get data export request status
        /// </summary>
        [HttpGet("export/requests/{requestId}/status")]
        public async Task<IActionResult> GetExportRequestStatus(string requestId)
        {
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                _logger.LogDebug("API: User {UserId} checking export status for request {RequestId}", userId, requestId);
                
                var requests = await _enhancedGdprService.GetUserExportRequestsAsync(userId);
                var request = requests.FirstOrDefault(r => r.Id == requestId);

                if (request == null)
                {
                    _logger.LogWarning("API: User {UserId} attempted to access export request {RequestId} that doesn't exist or doesn't belong to them", userId, requestId);
                    await AuditAsync("ApiExportStatusAccessDenied", "GdprApi", requestId.ToString(), "Attempted to access export request that doesn't exist or doesn't belong to user", userId, "Warning");
                    return NotFound(new { error = "Export request not found" });
                }
                
                _logger.LogDebug("API: User {UserId} checked export status for request {RequestId}: {Status}", userId, requestId, request.Status);
                await AuditAsync("ApiCheckExportStatus", "GdprApi", requestId.ToString(), $"Checked export status: {request.Status}", userId, "Information");
                
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error getting export request status for user {UserId}, request {RequestId}", currentUserId, requestId);
                await AuditAsync("ApiCheckExportStatusError", "GdprApi", requestId.ToString(), $"Error: {ex.Message}", currentUserId, "Error");
                
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
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                _logger.LogDebug("API: User {UserId} requesting data rectification: {DataType}.{FieldName}", userId, request.DataType, request.FieldName);
                
                var rectificationRequest = await _enhancedGdprService.CreateRectificationRequestAsync(
                    userId, request.DataType, request.FieldName, request.CurrentValue, request.RequestedValue, request.Reason);
                
                _logger.LogInformation("API: User {UserId} successfully requested data rectification {RequestId}: {DataType}.{FieldName}", userId, rectificationRequest.Id, request.DataType, request.FieldName);
                await AuditAsync("ApiRequestDataRectification", "GdprApi", rectificationRequest.Id.ToString(), $"Requested rectification: {request.DataType}.{request.FieldName} = {request.CurrentValue} -> {request.RequestedValue}", userId, "Information");
                
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error requesting data rectification for user {UserId}", currentUserId);
                await AuditAsync("ApiRequestDataRectificationError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get data rectification requests for the authenticated user
        /// </summary>
        [HttpGet("rectification/requests")]
        public async Task<IActionResult> GetDataRectificationRequests()
        {
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error getting data rectification requests for user {UserId}", currentUserId);
                await AuditAsync("ApiGetDataRectificationRequestsError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
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
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                _logger.LogDebug("API: User {UserId} requesting account deletion with reason: {Reason}", userId, request.Reason);
                
                var success = await _gdprService.RequestAccountDeletionAsync(userId, request.Reason);
                
                if (success)
                {
                    _logger.LogInformation("API: User {UserId} successfully requested account deletion", userId);
                    await AuditAsync("ApiRequestAccountDeletion", "GdprApi", null, $"Requested account deletion with reason: {request.Reason}", userId, "Information");
                    
                    return Ok(new
                    {
                        success = true,
                        message = "Account deletion request submitted successfully"
                    });
                }
                else
                {
                    _logger.LogWarning("API: User {UserId} failed to request account deletion", userId);
                    await AuditAsync("ApiRequestAccountDeletionFailed", "GdprApi", null, "Failed to request account deletion", userId, "Warning");
                    
                    return BadRequest(new { error = "Failed to submit account deletion request" });
                }
            }
            catch (Exception ex)
            {
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error requesting account deletion for user {UserId}", currentUserId);
                await AuditAsync("ApiRequestAccountDeletionError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Recover account from deletion
        /// </summary>
        [HttpPost("deletion/recover")]
        public async Task<IActionResult> RecoverAccount()
        {
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error recovering account for user {UserId}", currentUserId);
                await AuditAsync("ApiRecoverAccountError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
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
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error getting consent history for user {UserId}", currentUserId);
                await AuditAsync("ApiGetConsentHistoryError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Grant consent
        /// </summary>
        [HttpPost("consent/grant")]
        public async Task<IActionResult> GrantConsent([FromBody] ConsentRequestModel request)
        {
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error granting consent for user {UserId}", currentUserId);
                await AuditAsync("ApiGrantConsentError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Withdraw consent
        /// </summary>
        [HttpPost("consent/withdraw")]
        public async Task<IActionResult> WithdrawConsent([FromBody] ConsentWithdrawModel request)
        {
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error withdrawing consent for user {UserId}", currentUserId);
                await AuditAsync("ApiWithdrawConsentError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
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
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error getting processing logs for user {UserId}", currentUserId);
                await AuditAsync("ApiGetProcessingLogsError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
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
            var userId = _userManager?.GetUserId(User) ?? "Unknown";
            if (string.IsNullOrEmpty(userId) || userId == "Unknown")
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
                var currentUserId = _userManager?.GetUserId(User) ?? "Unknown";
                _logger.LogError(ex, "API: Error getting privacy dashboard for user {UserId}", currentUserId);
                await AuditAsync("ApiGetPrivacyDashboardError", "GdprApi", null, $"Error: {ex.Message}", currentUserId, "Error");
                
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