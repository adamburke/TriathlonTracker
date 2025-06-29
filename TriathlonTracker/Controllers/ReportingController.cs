using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using TriathlonTracker.Services;
using Microsoft.Extensions.Logging;

namespace TriathlonTracker.Controllers;

/// <summary>
/// Controller for reporting functionality. Provides endpoints for generating, viewing, and managing reports.
/// </summary>
[Authorize]
public class ReportingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAdminDashboardService _adminDashboardService;
    private readonly ILogger<ReportingController> _logger;
    private readonly IAuditService _auditService;

    /// <summary>
    /// Constructor for ReportingController.
    /// </summary>
    /// <param name="context">Application database context</param>
    /// <param name="env">Web host environment</param>
    /// <param name="httpContextAccessor">HTTP context accessor</param>
    /// <param name="adminDashboardService">Admin dashboard service</param>
    /// <param name="logger">Logger</param>
    /// <param name="auditService">Audit service</param>
    public ReportingController(ApplicationDbContext context, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, IAdminDashboardService adminDashboardService, ILogger<ReportingController> logger, IAuditService auditService)
    {
        _context = context;
        _env = env;
        _httpContextAccessor = httpContextAccessor;
        _adminDashboardService = adminDashboardService;
        _logger = logger;
        _auditService = auditService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogDebug("Admin {UserId} accessing reporting dashboard", userId);
            
            var dashboardData = await _adminDashboardService.GetDashboardDataAsync();
            var recentActivities = await _adminDashboardService.GetRecentActivitiesAsync(10);
            
            ViewBag.DashboardData = dashboardData;
            ViewBag.RecentActivities = recentActivities;
            
            _logger.LogInformation("Admin {UserId} accessed reporting dashboard with {ActivityCount} recent activities", userId, recentActivities.Count);
            await _auditService.LogAsync("ViewReportingDashboard", "Reporting", null, $"Viewed reporting dashboard with {recentActivities.Count} recent activities", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
            
            return View();
        }
        catch (Exception ex)
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogError(ex, "Error loading reporting dashboard for admin {UserId}", userId);
            await _auditService.LogAsync("ViewReportingDashboardError", "Reporting", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
            return View("Error");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateReport(string reportType, DateTime startDate, DateTime endDate, string format = "PDF")
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogDebug("Admin {UserId} generating {ReportType} report from {StartDate} to {EndDate} in {Format} format", userId, reportType, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), format);
            
            var report = await _adminDashboardService.GenerateComplianceReportAsync(reportType, startDate, endDate);
            
            if (report != null)
            {
                _logger.LogInformation("Admin {UserId} successfully generated {ReportType} report {ReportId} from {StartDate} to {EndDate}", userId, reportType, report.Id, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
                await _auditService.LogAsync("GenerateReport", "Reporting", report.Id.ToString(), $"Generated {reportType} report from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} in {format} format", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
                
                return Json(new { success = true, reportId = report.Id, message = "Report generated successfully" });
            }
            else
            {
                _logger.LogWarning("Admin {UserId} failed to generate {ReportType} report", userId, reportType);
                await _auditService.LogAsync("GenerateReportFailed", "Reporting", null, $"Failed to generate {reportType} report", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                
                return Json(new { success = false, message = "Failed to generate report" });
            }
        }
        catch (Exception ex)
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogError(ex, "Error generating {ReportType} report for admin {UserId}", reportType, userId);
            await _auditService.LogAsync("GenerateReportError", "Reporting", null, $"Error generating {reportType} report: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
            
            return Json(new { success = false, message = "An error occurred while generating the report" });
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DownloadReport(string reportId, string format = "PDF")
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogDebug("Admin {UserId} downloading report {ReportId} in {Format} format", userId, reportId, format);
            
            var fileBytes = await _adminDashboardService.ExportComplianceReportAsync(reportId, format);
            
            if (fileBytes == null || fileBytes.Length == 0)
            {
                _logger.LogWarning("Admin {UserId} attempted to download report {ReportId} with no file content", userId, reportId);
                await _auditService.LogAsync("DownloadReportNoContent", "Reporting", reportId, "Report has no file content", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                return NotFound("Report file not found");
            }

            var contentType = format.ToLower() switch
            {
                "pdf" => "application/pdf",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                "json" => "application/json",
                _ => "application/octet-stream"
            };

            var fileName = $"compliance_report_{reportId}_{DateTime.Now:yyyyMMdd}.{format.ToLower()}";

            _logger.LogInformation("Admin {UserId} successfully downloaded report {ReportId} ({FileName})", userId, reportId, fileName);
            await _auditService.LogAsync("DownloadReport", "Reporting", reportId, $"Downloaded report: {fileName}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogError(ex, "Error downloading report {ReportId} for admin {UserId}", reportId, userId);
            await _auditService.LogAsync("DownloadReportError", "Reporting", reportId, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
            
            return StatusCode(500, "An error occurred while downloading the report");
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReportHistory(int page = 1, int pageSize = 20)
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogDebug("Admin {UserId} viewing report history page {Page}", userId, page);
            
            var reports = await _adminDashboardService.GetReportsAsync(page, pageSize);
            
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalReports = reports.Count;
            ViewBag.TotalPages = (int)Math.Ceiling((double)reports.Count / pageSize);
            
            _logger.LogInformation("Admin {UserId} viewed report history page {Page} with {Count} reports", userId, page, reports.Count);
            await _auditService.LogAsync("ViewReportHistory", "Reporting", null, $"Viewed report history page {page} with {reports.Count} reports", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
            
            return View(reports);
        }
        catch (Exception ex)
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogError(ex, "Error loading report history for admin {UserId}", userId);
            await _auditService.LogAsync("ViewReportHistoryError", "Reporting", null, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
            return View("Error");
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ReportDetails(string reportId)
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogDebug("Admin {UserId} viewing details for report {ReportId}", userId, reportId);
            
            var reports = await _adminDashboardService.GetReportsAsync(1, 1000); // Get all reports to find the specific one
            var report = reports.FirstOrDefault(r => r.Id == reportId);
            
            if (report == null)
            {
                _logger.LogWarning("Admin {UserId} attempted to view non-existent report {ReportId}", userId, reportId);
                await _auditService.LogAsync("ViewReportDetailsNotFound", "Reporting", reportId, "Attempted to view non-existent report", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Warning");
                return NotFound("Report not found");
            }

            _logger.LogInformation("Admin {UserId} viewed details for report {ReportId}", userId, reportId);
            await _auditService.LogAsync("ViewReportDetails", "Reporting", reportId, $"Viewed details for compliance report", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Information");
            
            return View(report);
        }
        catch (Exception ex)
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogError(ex, "Error loading report details for admin {UserId}, report {ReportId}", userId, reportId);
            await _auditService.LogAsync("ViewReportDetailsError", "Reporting", reportId, $"Error: {ex.Message}", userId, HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"], "Error");
            return View("Error");
        }
    }
} 