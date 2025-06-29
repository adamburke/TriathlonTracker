using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using System.Xml.Linq;
using TriathlonTracker.Data;
using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public class EnhancedGdprService : IEnhancedGdprService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EnhancedGdprService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGdprService _baseGdprService;
        private readonly IConfiguration _configuration;

        public EnhancedGdprService(
            ApplicationDbContext context,
            ILogger<EnhancedGdprService> logger,
            IHttpContextAccessor httpContextAccessor,
            IGdprService baseGdprService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _baseGdprService = baseGdprService;
            _configuration = configuration;
        }

        #region Enhanced Data Export

        public async Task<DataExportRequest> CreateDataExportRequestAsync(string userId, string format)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var request = new DataExportRequest
                {
                    UserId = userId,
                    Format = format.ToUpper(),
                    RequestDate = DateTime.UtcNow,
                    Status = "Pending",
                    ExpirationDate = DateTime.UtcNow.AddDays(30), // 30-day expiration
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
                };

                _context.DataExportRequests.Add(request);
                await _context.SaveChangesAsync();

                await _baseGdprService.LogDataProcessingAsync(userId, "ExportRequested", "AllPersonalData", "UserRequest", "Consent", $"Format: {format}");

                _logger.LogInformation("Data export request created for user {UserId}, format {Format}", userId, format);
                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating data export request for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ProcessDataExportRequestAsync(int requestId)
        {
            try
            {
                var request = await _context.DataExportRequests.FindAsync(requestId);
                if (request == null || request.Status != "Pending")
                    return false;

                request.Status = "Processing";
                await _context.SaveChangesAsync();

                // Generate export data
                var exportData = await ExportUserDataInFormatAsync(request.UserId, request.Format);
                
                // Save to file system (in production, use cloud storage)
                var fileName = $"export_{request.UserId}_{DateTime.UtcNow:yyyyMMddHHmmss}.{request.Format.ToLower()}";
                var filePath = Path.Combine("wwwroot", "exports", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                await File.WriteAllBytesAsync(filePath, exportData);

                // Update request
                request.Status = "Completed";
                request.CompletedDate = DateTime.UtcNow;
                request.FileName = fileName;
                request.FileSizeBytes = exportData.Length;
                request.DownloadUrl = await GenerateSecureDownloadLinkAsync(requestId);

                await _context.SaveChangesAsync();

                await _baseGdprService.LogDataProcessingAsync(request.UserId, "ExportCompleted", "AllPersonalData", "UserRequest", "Consent", $"File: {fileName}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data export request {RequestId}", requestId);
                
                var request = await _context.DataExportRequests.FindAsync(requestId);
                if (request != null)
                {
                    request.Status = "Failed";
                    request.ErrorMessage = ex.Message;
                    await _context.SaveChangesAsync();
                }
                
                return false;
            }
        }

        public Task<string> GenerateSecureDownloadLinkAsync(int requestId)
        {
            var token = Guid.NewGuid().ToString("N");
            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:5001";
            return Task.FromResult($"{baseUrl}/Privacy/DownloadExport?token={token}&requestId={requestId}");
        }

        public async Task<IEnumerable<DataExportRequest>> GetUserExportRequestsAsync(string userId)
        {
            return await _context.DataExportRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<bool> CleanupExpiredExportRequestsAsync()
        {
            try
            {
                var expiredRequests = await _context.DataExportRequests
                    .Where(r => r.ExpirationDate < DateTime.UtcNow && r.Status == "Completed")
                    .ToListAsync();

                foreach (var request in expiredRequests)
                {
                    // Delete physical file
                    if (!string.IsNullOrEmpty(request.FileName))
                    {
                        var filePath = Path.Combine("wwwroot", "exports", request.FileName);
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }

                    request.Status = "Expired";
                    request.DownloadUrl = "";
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired export requests");
                return false;
            }
        }

        public async Task<byte[]> ExportUserDataInFormatAsync(string userId, string format)
        {
            var jsonData = await _baseGdprService.ExportUserDataAsync(userId, format);
            
            return format.ToUpper() switch
            {
                "JSON" => Encoding.UTF8.GetBytes(jsonData),
                "CSV" => ConvertJsonToCsv(jsonData),
                "XML" => ConvertJsonToXml(jsonData),
                _ => Encoding.UTF8.GetBytes(jsonData)
            };
        }

        #endregion

        #region Data Rectification System

        public async Task<DataRectificationRequest> CreateRectificationRequestAsync(string userId, string dataType, string fieldName, string currentValue, string requestedValue, string reason)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var request = new DataRectificationRequest
                {
                    UserId = userId,
                    DataType = dataType,
                    FieldName = fieldName,
                    CurrentValue = currentValue,
                    RequestedValue = requestedValue,
                    Reason = reason,
                    RequestDate = DateTime.UtcNow,
                    Status = "Pending",
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown",
                    Priority = DeterminePriority(dataType, fieldName)
                };

                _context.DataRectificationRequests.Add(request);
                await _context.SaveChangesAsync();

                await _baseGdprService.LogDataProcessingAsync(userId, "RectificationRequested", dataType, "UserRequest", "Consent", $"Field: {fieldName}");

                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rectification request for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ReviewRectificationRequestAsync(int requestId, bool approved, string reviewNotes, string reviewedBy)
        {
            try
            {
                var request = await _context.DataRectificationRequests.FindAsync(requestId);
                if (request == null || request.Status != "Pending")
                    return false;

                request.Status = approved ? "Approved" : "Rejected";
                request.ReviewDate = DateTime.UtcNow;
                request.ReviewedBy = reviewedBy;
                request.ReviewNotes = reviewNotes;
                
                if (!approved)
                    request.RejectionReason = reviewNotes;

                await _context.SaveChangesAsync();

                await _baseGdprService.LogDataProcessingAsync(request.UserId, "RectificationReviewed", request.DataType, "AdminAction", "LegitimateInterest", $"Approved: {approved}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing rectification request {RequestId}", requestId);
                return false;
            }
        }

        public async Task<bool> ProcessApprovedRectificationAsync(int requestId)
        {
            try
            {
                var request = await _context.DataRectificationRequests.FindAsync(requestId);
                if (request == null || request.Status != "Approved")
                    return false;

                // Apply the rectification based on data type
                var success = await ApplyDataRectificationAsync(request);
                
                if (success)
                {
                    request.Status = "Completed";
                    request.CompletedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await _baseGdprService.LogDataProcessingAsync(request.UserId, "RectificationCompleted", request.DataType, "AdminAction", "LegitimateInterest", $"Field: {request.FieldName}");
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing rectification request {RequestId}", requestId);
                return false;
            }
        }

        public async Task<IEnumerable<DataRectificationRequest>> GetUserRectificationRequestsAsync(string userId)
        {
            return await _context.DataRectificationRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<DataRectificationRequest>> GetPendingRectificationRequestsAsync()
        {
            return await _context.DataRectificationRequests
                .Where(r => r.Status == "Pending" || r.Status == "Approved")
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<bool> ValidateRectificationRequestAsync(DataRectificationRequest request)
        {
            // Implement validation logic based on data type and field
            return await Task.FromResult(true); // Simplified for now
        }

        #endregion

        #region Enhanced Account Deletion

        public async Task<AccountDeletionRequest> CreateAccountDeletionRequestAsync(string userId, string reason, string deletionType = "SoftDelete")
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var confirmationToken = Guid.NewGuid().ToString("N");
                
                var request = new AccountDeletionRequest
                {
                    UserId = userId,
                    RequestDate = DateTime.UtcNow,
                    Status = "Pending",
                    Reason = reason,
                    DeletionType = deletionType,
                    ConfirmationToken = confirmationToken,
                    TokenExpirationDate = DateTime.UtcNow.AddDays(7), // 7-day confirmation window
                    RecoveryPeriodDays = 30,
                    RecoveryDeadline = DateTime.UtcNow.AddDays(37), // 30 days + 7 days confirmation
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                    UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown"
                };

                _context.AccountDeletionRequests.Add(request);
                await _context.SaveChangesAsync();

                await _baseGdprService.LogDataProcessingAsync(userId, "DeletionRequested", "AccountData", "UserRequest", "Consent", $"Type: {deletionType}");

                return request;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account deletion request for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ConfirmAccountDeletionAsync(string confirmationToken)
        {
            try
            {
                var request = await _context.AccountDeletionRequests
                    .FirstOrDefaultAsync(r => r.ConfirmationToken == confirmationToken && r.Status == "Pending");

                if (request == null || request.TokenExpirationDate < DateTime.UtcNow)
                    return false;

                request.Status = "Confirmed";
                request.ConfirmationDate = DateTime.UtcNow;
                request.ScheduledDeletionDate = DateTime.UtcNow.AddDays(request.RecoveryPeriodDays);

                await _context.SaveChangesAsync();

                await _baseGdprService.LogDataProcessingAsync(request.UserId, "DeletionConfirmed", "AccountData", "UserRequest", "Consent");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming account deletion with token {Token}", confirmationToken);
                return false;
            }
        }

        public async Task<bool> ProcessAccountDeletionAsync(int requestId)
        {
            try
            {
                var request = await _context.AccountDeletionRequests.FindAsync(requestId);
                if (request == null || request.Status != "Confirmed")
                    return false;

                request.Status = "Processing";
                await _context.SaveChangesAsync();

                // Create data export before deletion if requested
                if (!request.HasDataExport)
                {
                    var exportRequest = await CreateDataExportRequestAsync(request.UserId, "JSON");
                    await ProcessDataExportRequestAsync(exportRequest.Id);
                    request.HasDataExport = true;
                    request.DataExportDate = DateTime.UtcNow;
                }

                // Perform deletion based on type
                bool success = request.DeletionType switch
                {
                    "HardDelete" => await _baseGdprService.ProcessAccountDeletionAsync(request.UserId),
                    "Anonymize" => await _baseGdprService.AnonymizeUserDataAsync(request.UserId),
                    _ => await PerformSoftDeleteAsync(request.UserId)
                };

                if (success)
                {
                    request.Status = "Completed";
                    request.CompletedDate = DateTime.UtcNow;
                    request.ProcessedBy = "System";
                }

                await _context.SaveChangesAsync();
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing account deletion request {RequestId}", requestId);
                return false;
            }
        }

        public async Task<bool> RecoverAccountAsync(string userId)
        {
            try
            {
                var request = await _context.AccountDeletionRequests
                    .Where(r => r.UserId == userId && r.Status == "Confirmed" && r.IsRecoveryPeriodActive)
                    .FirstOrDefaultAsync();

                if (request == null || request.RecoveryDeadline < DateTime.UtcNow)
                    return false;

                request.Status = "Cancelled";
                request.IsRecoveryPeriodActive = false;
                await _context.SaveChangesAsync();

                await _baseGdprService.LogDataProcessingAsync(userId, "DeletionCancelled", "AccountData", "UserRequest", "Consent");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recovering account for user {UserId}", userId);
                return false;
            }
        }

        public async Task<IEnumerable<AccountDeletionRequest>> GetPendingDeletionRequestsAsync()
        {
            return await _context.AccountDeletionRequests
                .Where(r => r.Status == "Confirmed" && r.ScheduledDeletionDate <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<bool> CleanupExpiredDeletionRequestsAsync()
        {
            try
            {
                var expiredRequests = await _context.AccountDeletionRequests
                    .Where(r => r.Status == "Pending" && r.TokenExpirationDate < DateTime.UtcNow)
                    .ToListAsync();

                foreach (var request in expiredRequests)
                {
                    request.Status = "Expired";
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired deletion requests");
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private byte[] ConvertJsonToCsv(string jsonData)
        {
            // Simplified CSV conversion - in production, use a proper CSV library
            var lines = new List<string> { "Type,Field,Value" };
            
            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(jsonData);
                foreach (var property in data.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var subProperty in property.Value.EnumerateObject())
                        {
                            lines.Add($"{property.Name},{subProperty.Name},{subProperty.Value}");
                        }
                    }
                    else
                    {
                        lines.Add($"Root,{property.Name},{property.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting JSON to CSV");
                lines.Add($"Error,Conversion,{ex.Message}");
            }

            return Encoding.UTF8.GetBytes(string.Join("\n", lines));
        }

        private byte[] ConvertJsonToXml(string jsonData)
        {
            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(jsonData);
                var xml = new XElement("UserData");
                
                foreach (var property in data.EnumerateObject())
                {
                    xml.Add(new XElement(property.Name, property.Value.ToString()));
                }

                return Encoding.UTF8.GetBytes(xml.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting JSON to XML");
                return Encoding.UTF8.GetBytes($"<Error>Failed to convert: {ex.Message}</Error>");
            }
        }

        private int DeterminePriority(string dataType, string fieldName)
        {
            // High priority for personal identifiable information
            if (dataType.ToLower().Contains("personal") || 
                fieldName.ToLower().Contains("email") || 
                fieldName.ToLower().Contains("name"))
                return 3;
            
            // Medium priority for other user data
            if (dataType.ToLower().Contains("user"))
                return 2;
            
            // Low priority for everything else
            return 1;
        }

        private async Task<bool> ApplyDataRectificationAsync(DataRectificationRequest request)
        {
            try
            {
                switch (request.DataType.ToLower())
                {
                    case "personaldata":
                        return await UpdatePersonalDataAsync(request);
                    case "triathlondata":
                        return await UpdateTriathlonDataAsync(request);
                    default:
                        _logger.LogWarning("Unknown data type for rectification: {DataType}", request.DataType);
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying data rectification for request {RequestId}", request.Id);
                return false;
            }
        }

        private async Task<bool> UpdatePersonalDataAsync(DataRectificationRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return false;

            switch (request.FieldName.ToLower())
            {
                case "firstname":
                    user.FirstName = request.RequestedValue;
                    break;
                case "lastname":
                    user.LastName = request.RequestedValue;
                    break;
                case "email":
                    user.Email = request.RequestedValue;
                    user.UserName = request.RequestedValue;
                    break;
                case "phonenumber":
                    user.PhoneNumber = request.RequestedValue;
                    break;
                default:
                    return false;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> UpdateTriathlonDataAsync(DataRectificationRequest request)
        {
            // Implementation would depend on specific triathlon data structure
            // This is a placeholder for the actual implementation
            return await Task.FromResult(true);
        }

        private async Task<bool> PerformSoftDeleteAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Mark user as deleted but keep data for recovery period
                user.Email = $"deleted_{user.Id}@deleted.local";
                user.UserName = user.Email;
                user.FirstName = "[DELETED]";
                user.LastName = "[DELETED]";
                user.PhoneNumber = null;
                user.IsAccountDeletionRequested = true;
                user.AccountDeletionRequestDate = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing soft delete for user {UserId}", userId);
                return false;
            }
        }

        #endregion

        #region Placeholder Methods (to be implemented)

        public Task<bool> ImportUserDataAsync(string userId, string jsonData) => Task.FromResult(false);
        public Task<bool> ValidateImportDataAsync(string jsonData) => Task.FromResult(false);
        public Task<object> GetDataPortabilityStatusAsync(string userId) => Task.FromResult<object>(new { });
        public Task<bool> CreateDataMigrationJobAsync(string fromUserId, string toUserId) => Task.FromResult(false);
        public Task<bool> CreateConsentVersionAsync(string version, string description, DateTime effectiveDate) => Task.FromResult(false);
        public Task<IEnumerable<ConsentRecord>> GetConsentHistoryWithVersionsAsync(string userId) => Task.FromResult(Enumerable.Empty<ConsentRecord>());
        public Task<bool> NotifyConsentRenewalRequiredAsync(string userId, string consentType) => Task.FromResult(false);
        public Task<object> GetConsentAnalyticsAsync(DateTime fromDate, DateTime toDate) => Task.FromResult<object>(new { });
        public Task<bool> BulkUpdateConsentVersionAsync(string oldVersion, string newVersion) => Task.FromResult(false);
        public Task<BreachIncident> CreateBreachIncidentAsync(string breachType, string severity, string description, string[] affectedDataTypes, string[] affectedUserIds) => Task.FromResult(new BreachIncident());
        public Task<bool> UpdateBreachIncidentAsync(int incidentId, string status, string notes, string updatedBy) => Task.FromResult(false);
        public Task<bool> NotifyUsersOfBreachAsync(int incidentId) => Task.FromResult(false);
        public Task<bool> NotifyRegulatorsOfBreachAsync(int incidentId) => Task.FromResult(false);
        public Task<IEnumerable<BreachIncident>> GetActiveBreachIncidentsAsync() => Task.FromResult(Enumerable.Empty<BreachIncident>());
        public Task<object> GetBreachStatisticsAsync(DateTime fromDate, DateTime toDate) => Task.FromResult<object>(new { });
        public Task<bool> DetectPotentialBreachAsync(string userId, string action, string ipAddress) => Task.FromResult(false);
        public Task<object> GetEnhancedPrivacyDashboardAsync(string userId) => Task.FromResult<object>(new { });
        public Task<object> GetDataUsageAnalyticsAsync(string userId, DateTime fromDate, DateTime toDate) => Task.FromResult<object>(new { });
        public Task<object> GetDataRetentionTimelineAsync(string userId) => Task.FromResult<object>(new { });
        public Task<IEnumerable<object>> GetPrivacySettingsAsync(string userId) => Task.FromResult(Enumerable.Empty<object>());
        public Task<bool> UpdatePrivacySettingsAsync(string userId, Dictionary<string, object> settings) => Task.FromResult(false);
        public Task<bool> ValidateApiRequestAsync(string apiKey, string endpoint) => Task.FromResult(false);
        public Task<bool> LogApiAccessAsync(string apiKey, string endpoint, string userId, bool success) => Task.FromResult(false);
        public Task<object> GetApiUsageStatisticsAsync(string apiKey, DateTime fromDate, DateTime toDate) => Task.FromResult<object>(new { });
        public Task<bool> RateLimitCheckAsync(string apiKey, string endpoint) => Task.FromResult(false);
        public Task<bool> SendGdprNotificationAsync(string userId, string notificationType, Dictionary<string, object> parameters) => Task.FromResult(false);
        public Task<bool> ScheduleGdprNotificationAsync(string userId, string notificationType, DateTime scheduledDate, Dictionary<string, object> parameters) => Task.FromResult(false);
        public Task<IEnumerable<object>> GetPendingNotificationsAsync() => Task.FromResult(Enumerable.Empty<object>());
        public Task<bool> ProcessPendingNotificationsAsync() => Task.FromResult(false);
        public Task<object> GenerateEnhancedComplianceReportAsync(DateTime fromDate, DateTime toDate) => Task.FromResult<object>(new { });
        public Task<object> GenerateDataSubjectRequestReportAsync(DateTime fromDate, DateTime toDate) => Task.FromResult<object>(new { });
        public Task<object> GenerateBreachReportAsync(DateTime fromDate, DateTime toDate) => Task.FromResult<object>(new { });
        public Task<object> GenerateConsentReportAsync(DateTime fromDate, DateTime toDate) => Task.FromResult<object>(new { });
        public Task<bool> ValidateUserDataIntegrityAsync(string userId) => Task.FromResult(false);
        public Task<object> GetDataQualityReportAsync(string userId) => Task.FromResult<object>(new { });
        public Task<bool> FixDataQualityIssuesAsync(string userId, string[] issueTypes) => Task.FromResult(false);
        public Task<bool> CreateAuditLogAsync(string action, string userId, string details, string performedBy) => Task.FromResult(false);
        public Task<IEnumerable<object>> GetAuditLogsAsync(DateTime fromDate, DateTime toDate, string? userId = null) => Task.FromResult(Enumerable.Empty<object>());
        public Task<bool> MonitorGdprComplianceAsync() => Task.FromResult(false);
        public Task<object> GetComplianceMetricsAsync() => Task.FromResult<object>(new { });

        #endregion
    }
}