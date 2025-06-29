using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TriathlonTracker.Models;
using TriathlonTracker.Services;
using Microsoft.Extensions.Logging;

namespace TriathlonTracker.Controllers
{
    [Route("Privacy/Enhanced")]
    public class EnhancedPrivacyController : BaseController
    {
        private readonly IEnhancedGdprService _enhancedGdprService;
        private readonly IGdprService _gdprService;

        public EnhancedPrivacyController(
            IEnhancedGdprService enhancedGdprService,
            IGdprService gdprService,
            UserManager<User> userManager,
            ILogger<EnhancedPrivacyController> logger,
            IAuditService auditService)
            : base(auditService, logger, userManager)
        {
            _enhancedGdprService = enhancedGdprService;
            _gdprService = gdprService;
        }

        #region Enhanced Data Export

        [HttpGet("DataExport")]
        [Authorize]
        public async Task<IActionResult> DataExport()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} accessing data export page", userId);
                var requests = await _enhancedGdprService.GetUserExportRequestsAsync(userId);
                
                _logger.LogInformation("User {UserId} viewed data export page with {Count} requests", userId, requests.Count());
                await AuditAsync("ViewDataExport", "DataExport", null, $"Viewed data export page with {requests.Count()} requests", userId, "Information");
                
                return View(requests);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error loading data export page for user {UserId}", userId);
                await AuditAsync("ViewDataExportError", "DataExport", null, $"Error: {ex.Message}", userId, "Error");
                return View("Error");
            }
        }

        [HttpPost("RequestDataExport")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestDataExport(string format = "JSON")
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} requesting data export in format {Format}", userId, format);
                
                var request = await _enhancedGdprService.CreateDataExportRequestAsync(userId, format);
                
                // Process the request asynchronously
                _ = Task.Run(async () => await _enhancedGdprService.ProcessDataExportRequestAsync(request.Id));

                _logger.LogInformation("User {UserId} successfully requested data export {RequestId} in format {Format}", userId, request.Id, format);
                await AuditAsync("RequestDataExport", "DataExport", request.Id.ToString(), $"Requested data export in {format} format", userId, "Information");

                return Json(new { 
                    success = true, 
                    message = "Data export request submitted successfully. You will be notified when it's ready for download.",
                    requestId = request.Id
                });
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error requesting data export for user {UserId}", userId);
                await AuditAsync("RequestDataExportError", "DataExport", null, $"Error: {ex.Message}", userId, "Error");
                return Json(new { success = false, message = "An error occurred while processing your request" });
            }
        }

        [HttpGet("DownloadExport")]
        [Authorize]
        public async Task<IActionResult> DownloadExport(int requestId, string token)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} attempting to download export {RequestId}", userId, requestId);
                
                var requests = await _enhancedGdprService.GetUserExportRequestsAsync(userId);
                var request = requests.FirstOrDefault(r => r.Id == requestId);

                if (request == null || request.Status != "Completed")
                {
                    _logger.LogWarning("User {UserId} attempted to download non-existent or incomplete export {RequestId}", userId, requestId);
                    await AuditAsync("DownloadExportNotFound", "DataExport", requestId.ToString(), "Attempted to download non-existent or incomplete export", userId, "Warning");
                    return NotFound("Export request not found or not ready");
                }

                if (string.IsNullOrEmpty(request.FileName))
                {
                    _logger.LogWarning("User {UserId} attempted to download export {RequestId} with no filename", userId, requestId);
                    await AuditAsync("DownloadExportNoFile", "DataExport", requestId.ToString(), "Export has no filename", userId, "Warning");
                    return NotFound("Export file not found");
                }

                var filePath = Path.Combine("wwwroot", "exports", request.FileName);
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("User {UserId} attempted to download export {RequestId} with missing file {FileName}", userId, requestId, request.FileName);
                    await AuditAsync("DownloadExportFileMissing", "DataExport", requestId.ToString(), $"File missing: {request.FileName}", userId, "Warning");
                    return NotFound("Export file not found on disk");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var contentType = request.Format.ToLower() switch
                {
                    "json" => "application/json",
                    "csv" => "text/csv",
                    "xml" => "application/xml",
                    _ => "application/octet-stream"
                };

                // Update download statistics
                request.DownloadCount++;
                request.LastDownloadDate = DateTime.UtcNow;
                // Note: In a real implementation, you'd update this in the database

                _logger.LogInformation("User {UserId} successfully downloaded export {RequestId} ({FileName})", userId, requestId, request.FileName);
                await AuditAsync("DownloadExport", "DataExport", requestId.ToString(), $"Downloaded export: {request.FileName}", userId, "Information");

                return File(fileBytes, contentType, request.FileName);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error downloading export for user {UserId}, request {RequestId}", userId, requestId);
                await AuditAsync("DownloadExportError", "DataExport", requestId.ToString(), $"Error: {ex.Message}", userId, "Error");
                return StatusCode(500, "An error occurred while downloading your export");
            }
        }

        #endregion

        #region Data Rectification

        [HttpGet("DataRectification")]
        [Authorize]
        public async Task<IActionResult> DataRectification()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} accessing data rectification page", userId);
                var requests = await _enhancedGdprService.GetUserRectificationRequestsAsync(userId);
                
                _logger.LogInformation("User {UserId} viewed data rectification page with {Count} requests", userId, requests.Count());
                await AuditAsync("ViewDataRectification", "DataRectification", null, $"Viewed data rectification page with {requests.Count()} requests", userId, "Information");
                
                return View(requests);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error loading data rectification page for user {UserId}", userId);
                await AuditAsync("ViewDataRectificationError", "DataRectification", null, $"Error: {ex.Message}", userId, "Error");
                return View("Error");
            }
        }

        [HttpPost("RequestDataRectification")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestDataRectification(
            string dataType, 
            string fieldName, 
            string currentValue, 
            string requestedValue, 
            string reason)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} requesting data rectification: {DataType}.{FieldName}", userId, dataType, fieldName);
                
                var request = await _enhancedGdprService.CreateRectificationRequestAsync(
                    userId, dataType, fieldName, currentValue, requestedValue, reason);

                _logger.LogInformation("User {UserId} successfully requested data rectification {RequestId}: {DataType}.{FieldName}", userId, request.Id, dataType, fieldName);
                await AuditAsync("RequestDataRectification", "DataRectification", request.Id.ToString(), $"Requested rectification: {dataType}.{fieldName} = {currentValue} -> {requestedValue}", userId, "Information");

                return Json(new { 
                    success = true, 
                    message = "Data rectification request submitted successfully. It will be reviewed by our team.",
                    requestId = request.Id
                });
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error requesting data rectification for user {UserId}", userId);
                await AuditAsync("RequestDataRectificationError", "DataRectification", null, $"Error: {ex.Message}", userId, "Error");
                return Json(new { success = false, message = "An error occurred while processing your request" });
            }
        }

        [HttpGet("RectificationStatus/{requestId}")]
        [Authorize]
        public async Task<IActionResult> RectificationStatus(int requestId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} checking status for rectification request {RequestId}", userId, requestId);
                
                var requests = await _enhancedGdprService.GetUserRectificationRequestsAsync(userId);
                var request = requests.FirstOrDefault(r => r.Id == requestId);

                if (request == null)
                {
                    _logger.LogWarning("User {UserId} attempted to check status for non-existent rectification request {RequestId}", userId, requestId);
                    await AuditAsync("RectificationStatusNotFound", "DataRectification", requestId.ToString(), "Attempted to check status for non-existent request", userId, "Warning");
                    return NotFound();
                }

                _logger.LogDebug("User {UserId} checked status for rectification request {RequestId}: {Status}", userId, requestId, request.Status);
                await AuditAsync("CheckRectificationStatus", "DataRectification", requestId.ToString(), $"Checked status: {request.Status}", userId, "Information");

                return Json(new {
                    status = request.Status,
                    requestDate = request.RequestDate,
                    reviewDate = request.ReviewDate,
                    completedDate = request.CompletedDate,
                    reviewNotes = request.ReviewNotes,
                    rejectionReason = request.RejectionReason
                });
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error checking rectification status for user {UserId}, request {RequestId}", userId, requestId);
                await AuditAsync("RectificationStatusError", "DataRectification", requestId.ToString(), $"Error: {ex.Message}", userId, "Error");
                return StatusCode(500, "An error occurred while checking the status");
            }
        }

        #endregion

        #region Enhanced Account Deletion

        [HttpGet("AccountDeletion")]
        [Authorize]
        public async Task<IActionResult> AccountDeletion()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("User {UserId} accessing account deletion page", userId);
                
                // Check if there's already a pending deletion request
                var existingRequest = await _gdprService.IsAccountDeletionRequestedAsync(userId);
                
                var model = new AccountDeletionViewModel
                {
                    HasPendingRequest = existingRequest,
                    DeletionTypes = new List<string> { "SoftDelete", "HardDelete", "Anonymize" }
                };

                _logger.LogInformation("User {UserId} viewed account deletion page, has pending request: {HasPending}", userId, existingRequest);
                await AuditAsync("ViewAccountDeletion", "AccountDeletion", null, $"Viewed account deletion page, pending request: {existingRequest}", userId, "Information");
                
                return View(model);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error loading account deletion page for user {UserId}", userId);
                await AuditAsync("ViewAccountDeletionError", "AccountDeletion", null, $"Error: {ex.Message}", userId, "Error");
                return View("Error");
            }
        }

        [HttpPost("RequestAccountDeletion")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestAccountDeletion(string reason, string deletionType = "SoftDelete")
        {
            var userId = GetCurrentUserId();
            try
            {
                var request = await _enhancedGdprService.CreateAccountDeletionRequestAsync(userId, reason, deletionType);

                return Json(new { 
                    success = true, 
                    message = "Account deletion request submitted. Please check your email for confirmation instructions.",
                    confirmationRequired = true,
                    requestId = request.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting account deletion for user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while processing your request" });
            }
        }

        [HttpGet("ConfirmDeletion")]
        public async Task<IActionResult> ConfirmDeletion(string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Invalid confirmation token");

            try
            {
                var success = await _enhancedGdprService.ConfirmAccountDeletionAsync(token);
                
                if (success)
                {
                    return View("DeletionConfirmed");
                }
                else
                {
                    return View("DeletionConfirmationFailed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming account deletion with token {Token}", token);
                return View("DeletionConfirmationFailed");
            }
        }

        [HttpPost("RecoverAccount")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecoverAccount()
        {
            var userId = GetCurrentUserId();
            try
            {
                var success = await _enhancedGdprService.RecoverAccountAsync(userId);
                
                if (success)
                {
                    return Json(new { 
                        success = true, 
                        message = "Account recovery successful. Your deletion request has been cancelled." 
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "Account recovery failed. The recovery period may have expired." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recovering account for user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while processing your request" });
            }
        }

        #endregion

        #region Enhanced Privacy Dashboard

        [HttpGet("Dashboard")]
        [Authorize]
        public async Task<IActionResult> EnhancedDashboard()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Account");

            try
            {
                var dashboardData = await _enhancedGdprService.GetEnhancedPrivacyDashboardAsync(userId);
                var dataUsage = await _enhancedGdprService.GetDataUsageAnalyticsAsync(userId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
                var retentionTimeline = await _enhancedGdprService.GetDataRetentionTimelineAsync(userId);
                var privacySettings = await _enhancedGdprService.GetPrivacySettingsAsync(userId);

                var model = new EnhancedPrivacyDashboardViewModel
                {
                    DashboardData = dashboardData,
                    DataUsageAnalytics = dataUsage,
                    RetentionTimeline = retentionTimeline,
                    PrivacySettings = privacySettings
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading enhanced privacy dashboard for user {UserId}", userId);
                return View("Error");
            }
        }

        [HttpPost("UpdatePrivacySettings")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrivacySettings([FromBody] Dictionary<string, object> settings)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Json(new { success = false, message = "User not authenticated" });

            try
            {
                var success = await _enhancedGdprService.UpdatePrivacySettingsAsync(userId, settings);
                
                return Json(new { 
                    success = success, 
                    message = success ? "Privacy settings updated successfully" : "Failed to update privacy settings" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating privacy settings for user {UserId}", userId);
                return Json(new { success = false, message = "An error occurred while updating your settings" });
            }
        }

        #endregion

        #region Admin Functions

        [HttpGet("Admin/RectificationRequests")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminRectificationRequests()
        {
            var requests = await _enhancedGdprService.GetPendingRectificationRequestsAsync();
            return View(requests);
        }

        [HttpPost("Admin/ReviewRectification")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewRectification(int requestId, bool approved, string reviewNotes)
        {
            var reviewedBy = _userManager?.GetUserName(User) ?? "Admin";
            
            try
            {
                var success = await _enhancedGdprService.ReviewRectificationRequestAsync(requestId, approved, reviewNotes, reviewedBy);
                
                if (success && approved)
                {
                    // Process the approved rectification
                    await _enhancedGdprService.ProcessApprovedRectificationAsync(requestId);
                }

                return Json(new { 
                    success = success, 
                    message = success ? "Rectification request reviewed successfully" : "Failed to review rectification request" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing rectification request {RequestId}", requestId);
                return Json(new { success = false, message = "An error occurred while reviewing the request" });
            }
        }

        [HttpGet("Admin/DeletionRequests")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeletionRequests()
        {
            var requests = await _enhancedGdprService.GetPendingDeletionRequestsAsync();
            return View(requests);
        }

        [HttpPost("Admin/ProcessDeletion")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessDeletion(int requestId)
        {
            try
            {
                var success = await _enhancedGdprService.ProcessAccountDeletionAsync(requestId);
                
                return Json(new { 
                    success = success, 
                    message = success ? "Account deletion processed successfully" : "Failed to process account deletion" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing account deletion request {RequestId}", requestId);
                return Json(new { success = false, message = "An error occurred while processing the deletion" });
            }
        }

        #endregion
    }

    #region View Models

    public class AccountDeletionViewModel
    {
        public bool HasPendingRequest { get; set; }
        public List<string> DeletionTypes { get; set; } = new();
    }

    public class EnhancedPrivacyDashboardViewModel
    {
        public object DashboardData { get; set; } = new();
        public object DataUsageAnalytics { get; set; } = new();
        public object RetentionTimeline { get; set; } = new();
        public IEnumerable<object> PrivacySettings { get; set; } = new List<object>();
    }

    #endregion
}