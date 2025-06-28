namespace TriathlonTracker.Services
{
    public interface IConfigurationService
    {
        Task<string?> GetValueAsync(string key);
        Task SetValueAsync(string key, string value, string? description = null);
        Task<bool> ExistsAsync(string key);
    }
}