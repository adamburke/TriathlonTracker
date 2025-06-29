using TriathlonTracker.Models;

namespace TriathlonTracker.Services.Interfaces
{
    public interface IRetentionService
    {
        Task<DataRetentionSummary> GetRetentionSummaryAsync();
        Task<bool> ExecuteRetentionPolicyAsync(string policyId);
        Task<List<RetentionPolicyStatus>> GetRetentionPolicyStatusesAsync();
        Task<bool> ScheduleDataCleanupAsync(DateTime scheduledDate);
        Task<List<string>> GetExpiredDataSummaryAsync();
    }
} 