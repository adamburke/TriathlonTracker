using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TriathlonTracker.Data;
using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public class DatabaseConfigurationService : IConfigurationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<DatabaseConfigurationService> _logger;
        
        // Keys that should be encrypted
        private readonly HashSet<string> _sensitiveKeys = new()
        {
            "Authentication:Google:ClientId",
            "Authentication:Google:ClientSecret",
            "Jwt:Key"
        };

        public DatabaseConfigurationService(ApplicationDbContext context, IEncryptionService encryptionService, ILogger<DatabaseConfigurationService> logger)
        {
            _context = context;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            try
            {
                var config = await _context.Configurations
                    .FirstOrDefaultAsync(c => c.Key == key);
                
                if (config == null)
                    return null;

                // Decrypt if the value is encrypted
                if (config.IsEncrypted)
                {
                    try
                    {
                        return _encryptionService.Decrypt(config.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to decrypt configuration value for key {Key}", key);
                        return null;
                    }
                }
                
                return config.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve configuration value for key {Key}", key);
                return null;
            }
        }

        public async Task SetValueAsync(string key, string value, string? description = null)
        {
            try
            {
                var shouldEncrypt = _sensitiveKeys.Contains(key);
                string valueToStore;
                
                if (shouldEncrypt)
                {
                    try
                    {
                        valueToStore = _encryptionService.Encrypt(value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to encrypt configuration value for key {Key}", key);
                        throw new InvalidOperationException($"Failed to encrypt sensitive configuration value for key: {key}", ex);
                    }
                }
                else
                {
                    valueToStore = value;
                }
                
                var config = await _context.Configurations
                    .FirstOrDefaultAsync(c => c.Key == key);

                if (config == null)
                {
                    config = new Configuration
                    {
                        Key = key,
                        Value = valueToStore,
                        Description = description,
                        IsEncrypted = shouldEncrypt,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Configurations.Add(config);
                }
                else
                {
                    config.Value = valueToStore;
                    config.Description = description ?? config.Description;
                    config.IsEncrypted = shouldEncrypt;
                    config.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogDebug("Configuration value set for key {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set configuration value for key {Key}", key);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _context.Configurations
                    .AnyAsync(c => c.Key == key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if configuration exists for key {Key}", key);
                return false;
            }
        }
    }
}