using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriathlonTracker.Models;
using TriathlonTracker.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace TriathlonTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin")]
    public class AdminController : BaseController
    {
        private readonly IAdminDashboardService _adminDashboardService;
        private readonly IGdprMonitoringService _monitoringService;
        private readonly IDataRetentionService _retentionService;
        private readonly ISecurityService _securityService;

        public AdminController(
            IAdminDashboardService adminDashboardService,
            IGdprMonitoringService monitoringService,
            IDataRetentionService retentionService,
            ISecurityService securityService,
            ILogger<AdminController> logger,
            IAuditService auditService,
            UserManager<User>? userManager = null)
            : base(auditService, logger, userManager)
        {
            _adminDashboardService = adminDashboardService;
            _monitoringService = monitoringService;
            _retentionService = retentionService;
            _securityService = securityService;
        }

        [HttpGet("")]
        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("Admin dashboard accessed by {UserId}", userId);
                
                var dashboardData = await _adminDashboardService.GetDashboardDataAsync();
                var systemStatus = await _adminDashboardService.GetSystemStatusAsync();
                
                ViewBag.SystemStatus = systemStatus;
                
                _logger.LogInformation("Admin {UserId} accessed dashboard", userId);
                await AuditAsync("ViewAdminDashboard", "Admin", null, "Admin dashboard accessed", userId, "Information");
                
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error loading admin dashboard for {UserId}", userId);
                await AuditAsync("ViewAdminDashboardError", "Admin", null, $"Error: {ex.Message}", userId, "Error");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpGet("Users")]
        public async Task<IActionResult> Users(int page = 1, int pageSize = 50, string? search = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogDebug("Admin {UserId} viewing users page {Page} with search: {Search}", userId, page, search ?? "none");
                
                var users = string.IsNullOrEmpty(search) 
                    ? await _adminDashboardService.GetUserGdprStatusesAsync(page, pageSize)
                    : await _adminDashboardService.SearchUsersAsync(search, page, pageSize);
                
                _logger.LogInformation("Admin {UserId} viewed users page {Page} with {Count} results", userId, page, users.Count());
                await AuditAsync("ViewUsers", "UserManagement", null, $"Viewed users page {page} with {users.Count()} results", userId, "Information");
                
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;
                
                return View(users);
            }
            catch (Exception ex)
            {
                var userId = GetCurrentUserId();
                _logger.LogError(ex, "Error loading users page for admin {UserId}", userId);
                await AuditAsync("ViewUsersError", "UserManagement", null, $"Error: {ex.Message}", userId, "Error");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpGet("User/{userId}")]
        public async Task<IActionResult> UserDetails(string userId)
        {
            try
            {
                var adminId = GetCurrentUserId();
                _logger.LogDebug("Admin {AdminId} viewing details for user {UserId}", adminId, userId);
                
                var userStatus = await _adminDashboardService.GetUserGdprStatusAsync(userId);
                if (userStatus == null)
                {
                    _logger.LogWarning("Admin {AdminId} attempted to view non-existent user {UserId}", adminId, userId);
                    await AuditAsync("ViewUserDetailsNotFound", "User", userId, "Attempted to view non-existent user", adminId, "Warning");
                    return NotFound();
                }

                _logger.LogInformation("Admin {AdminId} viewed details for user {UserId}", adminId, userId);
                await AuditAsync("ViewUserDetails", "User", userId, "Viewed user GDPR details", adminId, "Information");
                
                return View(userStatus);
            }
            catch (Exception ex)
            {
                var adminId = GetCurrentUserId();
                _logger.LogError(ex, "Error loading user details for {UserId} by admin {AdminId}", userId, adminId);
                await AuditAsync("ViewUserDetailsError", "User", userId, $"Error: {ex.Message}", adminId, "Error");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpGet("User/{userId}/Edit")]
        public async Task<IActionResult> EditUser(string userId)
        {
            try
            {
                var adminId = GetCurrentUserId();
                _logger.LogDebug("Admin {AdminId} accessing edit form for user {UserId}", adminId, userId);
                
                var userStatus = await _adminDashboardService.GetUserGdprStatusAsync(userId);
                if (userStatus == null)
                {
                    _logger.LogWarning("Admin {AdminId} attempted to edit non-existent user {UserId}", adminId, userId);
                    await AuditAsync("EditUserNotFound", "User", userId, "Attempted to edit non-existent user", adminId, "Warning");
                    return NotFound();
                }

                _logger.LogInformation("Admin {AdminId} accessed edit form for user {UserId}", adminId, userId);
                await AuditAsync("ViewUserEdit", "User", userId, "Viewed user edit form", adminId, "Information");
                
                return View("EditUser", userStatus);
            }
            catch (Exception ex)
            {
                var adminId = GetCurrentUserId();
                _logger.LogError(ex, "Error loading user edit form for {UserId} by admin {AdminId}", userId, adminId);
                await AuditAsync("EditUserError", "User", userId, $"Error: {ex.Message}", adminId, "Error");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpPost("User/{userId}/Edit")]
        public async Task<IActionResult> EditUser(string userId, string firstName, string lastName, string email, bool hasConsent)
        {
            try
            {
                var adminId = GetCurrentUserId();
                _logger.LogDebug("Admin {AdminId} updating user {UserId}", adminId, userId);
                
                var success = await _adminDashboardService.UpdateUserDetailsAsync(userId, firstName, lastName, email, hasConsent);
                if (success)
                {
                    _logger.LogInformation("Admin {AdminId} successfully updated user {UserId}", adminId, userId);
                    await AuditAsync("EditUser", "User", userId, $"Updated user details: {firstName} {lastName}, {email}, consent: {hasConsent}", adminId, "Information");
                    TempData["Success"] = "User updated successfully.";
                }
                else
                {
                    _logger.LogWarning("Admin {AdminId} failed to update user {UserId}", adminId, userId);
                    await AuditAsync("EditUserFailed", "User", userId, "Failed to update user details", adminId, "Warning");
                    TempData["Error"] = "Failed to update user.";
                }

                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                var adminId = GetCurrentUserId();
                _logger.LogError(ex, "Error updating user {UserId} by admin {AdminId}", userId, adminId);
                await AuditAsync("EditUserError", "User", userId, $"Error: {ex.Message}", adminId, "Error");
                TempData["Error"] = "An error occurred while updating the user.";
                return RedirectToAction("Users");
            }
        }

        [HttpPost("User/{userId}/UpdateConsent")]
        public async Task<IActionResult> UpdateUserConsent(string userId, string consentType, bool isGranted, string reason)
        {
            try
            {
                var adminId = GetCurrentUserId();
                _logger.LogDebug("Admin {AdminId} updating consent for user {UserId}: {ConsentType} = {IsGranted}", adminId, userId, consentType, isGranted);
                
                var success = await _adminDashboardService.UpdateUserConsentAsync(userId, consentType, isGranted, reason);
                if (success)
                {
                    _logger.LogInformation("Admin {AdminId} successfully updated consent for user {UserId}: {ConsentType} = {IsGranted}", adminId, userId, consentType, isGranted);
                    await AuditAsync("UpdateConsent", "ConsentRecord", userId, $"Updated consent: {consentType} = {isGranted}, reason: {reason}", adminId, "Information");
                    TempData["Success"] = "User consent updated successfully.";
                }
                else
                {
                    _logger.LogWarning("Admin {AdminId} failed to update consent for user {UserId}", adminId, userId);
                    await AuditAsync("UpdateConsentFailed", "ConsentRecord", userId, $"Failed to update consent: {consentType}", adminId, "Warning");
                    TempData["Error"] = "Failed to update user consent.";
                }

                return RedirectToAction("UserDetails", new { userId });
            }
            catch (Exception ex)
            {
                var adminId = GetCurrentUserId();
                _logger.LogError(ex, "Error updating consent for user {UserId} by admin {AdminId}", userId, adminId);
                await AuditAsync("UpdateConsentError", "ConsentRecord", userId, $"Error: {ex.Message}", adminId, "Error");
                TempData["Error"] = "An error occurred while updating consent.";
                return RedirectToAction("UserDetails", new { userId });
            }
        }

        [HttpPost("BulkConsent")]
        public async Task<IActionResult> BulkConsentOperation([FromBody] BulkConsentOperation operation)
        {
            try
            {
                var adminId = GetCurrentUserId();
                _logger.LogDebug("Admin {AdminId} performing bulk consent operation: {Operation} for {Count} users", adminId, operation.Operation, operation.UserIds.Count);
                
                var success = await _adminDashboardService.BulkUpdateConsentAsync(operation);
                
                if (success)
                {
                    _logger.LogInformation("Admin {AdminId} successfully performed bulk consent operation: {Operation} for {Count} users", adminId, operation.Operation, operation.UserIds.Count);
                    await AuditAsync("BulkConsentOperation", "ConsentRecord", null, $"Bulk operation: {operation.Operation} for {operation.UserIds.Count} users", adminId, "Information");
                }
                else
                {
                    _logger.LogWarning("Admin {AdminId} failed to perform bulk consent operation: {Operation}", adminId, operation.Operation);
                    await AuditAsync("BulkConsentOperationFailed", "ConsentRecord", null, $"Failed bulk operation: {operation.Operation} for {operation.UserIds.Count} users", adminId, "Warning");
                }
                
                return Json(new { success, message = success ? "Bulk operation completed successfully." : "Bulk operation failed." });
            }
            catch (Exception ex)
            {
                var adminId = GetCurrentUserId();
                _logger.LogError(ex, "Error performing bulk consent operation by admin {AdminId}", adminId);
                await AuditAsync("BulkConsentOperationError", "ConsentRecord", null, $"Error: {ex.Message}", adminId, "Error");
                return Json(new { success = false, message = "An error occurred during bulk operation." });
            }
        }

        [HttpGet("Compliance")]
        public async Task<IActionResult> Compliance()
        {
            try
            {
                var adminId = GetCurrentUserId();
                _logger.LogDebug("Admin {AdminId} accessing compliance dashboard", adminId);
                
                var complianceStatus = await _adminDashboardService.GetComplianceStatusAsync();
                var alerts = await _adminDashboardService.GetActiveAlertsAsync();
                
                ViewBag.ComplianceStatus = complianceStatus;
                ViewBag.Alerts = alerts;
                
                _logger.LogInformation("Admin {AdminId} accessed compliance dashboard with {AlertCount} active alerts", adminId, alerts.Count());
                await AuditAsync("ViewCompliance", "Compliance", null, $"Viewed compliance dashboard with {alerts.Count()} active alerts", adminId, "Information");
                
                return View();
            }
            catch (Exception ex)
            {
                var adminId = GetCurrentUserId();
                _logger.LogError(ex, "Error loading compliance dashboard for admin {AdminId}", adminId);
                await AuditAsync("ViewComplianceError", "Compliance", null, $"Error: {ex.Message}", adminId, "Error");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpPost("Compliance/RunCheck")]
        public async Task<IActionResult> RunComplianceCheck()
        {
            try
            {
                var success = await _adminDashboardService.RunComplianceCheckAsync();
                await AuditAsync("RunComplianceCheck", "Compliance", "", "Manual compliance check executed", null, "Information");
                
                return Json(new { success, message = success ? "Compliance check completed." : "Compliance check failed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running compliance check");
                return Json(new { success = false, message = "Error running compliance check." });
            }
        }

        [HttpPost("Alert/{alertId}/Resolve")]
        public async Task<IActionResult> ResolveAlert(string alertId, string resolutionNotes)
        {
            try
            {
                var success = await _adminDashboardService.ResolveAlertAsync(alertId, resolutionNotes);
                await AuditAsync("ResolveAlert", "ComplianceAlert", alertId, $"Alert resolved: {resolutionNotes}", null, "Information");
                
                return Json(new { success, message = success ? "Alert resolved successfully." : "Failed to resolve alert." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving alert {AlertId}", alertId);
                return Json(new { success = false, message = "Error resolving alert." });
            }
        }

        [HttpGet("DataRetention")]
        public async Task<IActionResult> DataRetention()
        {
            try
            {
                var retentionSummary = await _adminDashboardService.GetRetentionSummaryAsync();
                var jobs = await _retentionService.GetRetentionJobsAsync();
                var expiredData = await _adminDashboardService.GetExpiredDataSummaryAsync();
                
                ViewBag.RetentionSummary = retentionSummary;
                ViewBag.RetentionJobs = jobs;
                ViewBag.ExpiredData = expiredData;
                
                await AuditAsync("ViewDataRetention", "DataRetention", "", "Viewed data retention dashboard", null, "Information");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data retention page");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpPost("DataRetention/ExecutePolicy/{policyId}")]
        public async Task<IActionResult> ExecuteRetentionPolicy(string policyId)
        {
            try
            {
                var success = await _adminDashboardService.ExecuteRetentionPolicyAsync(policyId);
                await AuditAsync("ExecuteRetentionPolicy", "DataRetentionPolicy", policyId, "Manual retention policy execution", null, "Information");
                
                return Json(new { success, message = success ? "Retention policy executed successfully." : "Failed to execute retention policy." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing retention policy {PolicyId}", policyId);
                return Json(new { success = false, message = "Error executing retention policy." });
            }
        }

        [HttpGet("Reports")]
        public async Task<IActionResult> Reports(int page = 1, int pageSize = 20)
        {
            try
            {
                var reports = await _adminDashboardService.GetReportsAsync(page, pageSize);
                await AuditAsync("ViewReports", "Reports", "", $"Viewed reports page {page}", null, "Information");
                
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                
                return View(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports page");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpPost("Reports/Generate")]
        public async Task<IActionResult> GenerateReport(string reportType, DateTime startDate, DateTime endDate)
        {
            try
            {
                var report = await _adminDashboardService.GenerateComplianceReportAsync(reportType, startDate, endDate);
                await AuditAsync("GenerateReport", "ComplianceReport", report.Id, $"Generated {reportType} report", null, "Information");
                
                return Json(new { success = true, reportId = report.Id, message = "Report generated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return Json(new { success = false, message = "Error generating report." });
            }
        }

        [HttpGet("Reports/{reportId}/Export/{format}")]
        public async Task<IActionResult> ExportReport(string reportId, string format)
        {
            try
            {
                var reportData = await _adminDashboardService.ExportComplianceReportAsync(reportId, format);
                await AuditAsync("ExportReport", "ComplianceReport", reportId, $"Exported report in {format} format", null, "Information");
                
                var contentType = format.ToLower() switch
                {
                    "pdf" => "application/pdf",
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    _ => "application/json"
                };
                
                var fileName = $"compliance-report-{reportId}.{format.ToLower()}";
                return File(reportData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report {ReportId}", reportId);
                return NotFound();
            }
        }

        [HttpGet("Analytics")]
        public async Task<IActionResult> Analytics(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;
                
                var consentAnalytics = await _adminDashboardService.GetConsentAnalyticsAsync(start, end);
                var processingAnalytics = await _adminDashboardService.GetDataProcessingAnalyticsAsync(start, end);
                var breachAnalytics = await _adminDashboardService.GetBreachAnalyticsAsync(start, end);
                
                ViewBag.ConsentAnalytics = consentAnalytics;
                ViewBag.ProcessingAnalytics = processingAnalytics;
                ViewBag.BreachAnalytics = breachAnalytics;
                ViewBag.StartDate = start;
                ViewBag.EndDate = end;
                
                await AuditAsync("ViewAnalytics", "Analytics", "", $"Viewed analytics for {start:yyyy-MM-dd} to {end:yyyy-MM-dd}", null, "Information");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics page");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpGet("AuditLog")]
        public async Task<IActionResult> AuditLog(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 50, string? search = null)
        {
            try
            {
                var auditLogs = string.IsNullOrEmpty(search)
                    ? await _adminDashboardService.GetAuditLogsAsync(userId, startDate, endDate, page, pageSize)
                    : await _adminDashboardService.SearchAuditLogsAsync(search, page, pageSize);
                
                ViewBag.UserId = userId;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;
                
                await AuditAsync("ViewAuditLog", "AuditLog", "", "Viewed audit log", userId, "Information");
                return View(auditLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading audit log");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpGet("Security")]
        public async Task<IActionResult> Security()
        {
            try
            {
                var securityStatus = await _securityService.GetSecurityStatusAsync();
                var recentEvents = await _securityService.GetRecentSecurityEventsAsync(20);
                var ipControls = await _securityService.GetIpAccessControlsAsync();
                
                ViewBag.SecurityStatus = securityStatus;
                ViewBag.RecentEvents = recentEvents;
                ViewBag.IpControls = ipControls;
                
                await AuditAsync("ViewSecurity", "Security", "", "Viewed security dashboard", null, "Information");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading security page");
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
            }
        }

        [HttpGet("ExportUsers")]
        public async Task<IActionResult> ExportUsers(string format = "csv")
        {
            try
            {
                var users = await _adminDashboardService.GetUserGdprStatusesAsync(1, int.MaxValue);
                await AuditAsync("ExportUsers", "User", "", $"Exported users in {format} format", null, "Information");
                
                if (format.ToLower() == "excel")
                {
                    // For now, return CSV format - Excel export would need additional library
                    var csvData = GenerateUsersCsv(users);
                    return File(System.Text.Encoding.UTF8.GetBytes(csvData), "text/csv", "users-export.csv");
                }
                else
                {
                    var csvData = GenerateUsersCsv(users);
                    return File(System.Text.Encoding.UTF8.GetBytes(csvData), "text/csv", "users-export.csv");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting users");
                TempData["Error"] = "Failed to export users.";
                return RedirectToAction("Users");
            }
        }

        [HttpPost("User/{userId}/Delete")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var success = await _adminDashboardService.DeleteUserAsync(userId);
                await AuditAsync("DeleteUser", "User", userId, "User account deleted", null, success ? "Information" : "Warning");
                
                if (success)
                {
                    TempData["Success"] = "User deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to delete user.";
                }
                
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                TempData["Error"] = "An error occurred while deleting the user.";
                return RedirectToAction("Users");
            }
        }

        [HttpGet("ExportAnalytics")]
        public async Task<IActionResult> ExportAnalytics(DateTime? startDate = null, DateTime? endDate = null, string format = "excel")
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;
                
                var analyticsData = await _adminDashboardService.ExportAnalyticsDataAsync(start, end, format);
                await AuditAsync("ExportAnalytics", "Analytics", "", $"Exported analytics data in {format} format", null, "Information");
                
                var contentType = format.ToLower() switch
                {
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    _ => "application/json"
                };
                
                var fileName = $"analytics-export-{start:yyyy-MM-dd}-to-{end:yyyy-MM-dd}.{format.ToLower()}";
                return File(analyticsData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting analytics");
                TempData["Error"] = "Failed to export analytics data.";
                return RedirectToAction("Analytics");
            }
        }

        private string GenerateUsersCsv(IEnumerable<UserGdprStatus> users)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("UserId,Name,Email,HasConsent,ConsentDate,LastDataAccess,PendingRequests,HasRetentionViolation,CreatedDate");
            
            foreach (var user in users)
            {
                csv.AppendLine($"{user.UserId},{user.Name},{user.Email},{user.HasConsent},{user.ConsentDate?.ToString("yyyy-MM-dd")},{user.LastDataAccess?.ToString("yyyy-MM-dd")},{user.PendingRequests},{user.HasRetentionViolation},{user.CreatedDate:yyyy-MM-dd}");
            }
            
            return csv.ToString();
        }
    }
}