using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public interface IDataRetentionService
    {
        Task<IEnumerable<RetentionJob>> GetRetentionJobsAsync();
        Task<RetentionJob> CreateRetentionJobAsync(string policyId, DateTime scheduledDate);
        Task<bool> ExecuteRetentionJobAsync(string jobId);
        Task<RetentionSummary> GetRetentionSummaryAsync();
        Task<IEnumerable<ExpiredDataSummary>> GetExpiredDataAsync();
        Task<bool> CleanupExpiredDataAsync(string dataType, DateTime cutoffDate);
        Task<DataRetentionPolicy?> GetRetentionPolicyAsync(string policyId);
        Task<IEnumerable<DataRetentionPolicy>> GetActiveRetentionPoliciesAsync();
    }
}