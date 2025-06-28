using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using System.Security.Cryptography;
using System.Text;

namespace TriathlonTracker.Services
{
    public class DatabaseConfigurationProvider : ConfigurationProvider
    {
        private readonly string _connectionString;
        private readonly IEncryptionService _encryptionService;

        public DatabaseConfigurationProvider(string connectionString)
        {
            _connectionString = connectionString;
            // Create a simple encryption service for the configuration provider
            _encryptionService = new SimpleEncryptionService();
        }

        public override void Load()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(_connectionString);

            using var context = new ApplicationDbContext(optionsBuilder.Options);
            
            try
            {
                // Ensure database exists
                context.Database.EnsureCreated();
                
                // Load all configurations
                var configurations = context.Configurations.ToList();
                
                foreach (var config in configurations)
                {
                    // Decrypt if encrypted
                    var value = config.IsEncrypted ? _encryptionService.Decrypt(config.Value) : config.Value;
                    Data[config.Key] = value;
                }
            }
            catch
            {
                // If database is not available, continue without database config
            }
        }
    }

    // Simple encryption service for configuration provider (before DI is available)
    internal class SimpleEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private const string EncryptedPrefix = "ENC:";

        public SimpleEncryptionService()
        {
            var keyString = "TriathlonTracker2024SecretKey32"; // 32 chars for AES-256
            _key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
            
            var ivString = "TriathlonIV16Bit"; // 16 chars for AES IV
            _iv = Encoding.UTF8.GetBytes(ivString.PadRight(16).Substring(0, 16));
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText) || IsEncrypted(plainText))
                return plainText;

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);
            
            swEncrypt.Write(plainText);
            swEncrypt.Close();
            
            var encrypted = msEncrypt.ToArray();
            return EncryptedPrefix + Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText) || !IsEncrypted(cipherText))
                return cipherText;

            try
            {
                var encryptedData = Convert.FromBase64String(cipherText.Substring(EncryptedPrefix.Length));

                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(encryptedData);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                
                return srDecrypt.ReadToEnd();
            }
            catch
            {
                return cipherText;
            }
        }

        public bool IsEncrypted(string value)
        {
            return !string.IsNullOrEmpty(value) && value.StartsWith(EncryptedPrefix);
        }
    }

    public class DatabaseConfigurationSource : IConfigurationSource
    {
        private readonly string _connectionString;

        public DatabaseConfigurationSource(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DatabaseConfigurationProvider(_connectionString);
        }
    }

    public static class DatabaseConfigurationExtensions
    {
        public static IConfigurationBuilder AddDatabase(this IConfigurationBuilder builder, string connectionString)
        {
            return builder.Add(new DatabaseConfigurationSource(connectionString));
        }
    }
}