using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Models;

namespace TriathlonTracker.Services
{
    public class DatabaseConfigurationService : IConfigurationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEncryptionService _encryptionService;
        
        // Keys that should be encrypted
        private readonly HashSet<string> _sensitiveKeys = new()
        {
            "Authentication:Google:ClientId",
            "Authentication:Google:ClientSecret",
            "Jwt:Key"
        };

        public DatabaseConfigurationService(ApplicationDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var config = await _context.Configurations
                .FirstOrDefaultAsync(c => c.Key == key);
            
            if (config == null)
                return null;

            // Decrypt if the value is encrypted
            return config.IsEncrypted ? _encryptionService.Decrypt(config.Value) : config.Value;
        }

        public async Task SetValueAsync(string key, string value, string? description = null)
        {
            var shouldEncrypt = _sensitiveKeys.Contains(key);
            var valueToStore = shouldEncrypt ? _encryptionService.Encrypt(value) : value;
            
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
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _context.Configurations
                .AnyAsync(c => c.Key == key);
        }
    }
}