using TriathlonTracker.Models;

namespace TriathlonTracker.Services.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserGdprStatus>> GetUserGdprStatusesAsync(int page = 1, int pageSize = 50);
        Task<UserGdprStatus?> GetUserGdprStatusAsync(string userId);
        Task<bool> UpdateUserConsentAsync(string userId, string consentType, bool isGranted, string reason);
        Task<bool> UpdateUserDetailsAsync(string userId, string firstName, string lastName, string email, bool hasConsent);
        Task<bool> BulkUpdateConsentAsync(BulkConsentOperation operation);
        Task<List<UserGdprStatus>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 50);
        Task<bool> DeleteUserAsync(string userId);
        Task<byte[]> ExportAnalyticsDataAsync(DateTime startDate, DateTime endDate, string format);
    }
} 