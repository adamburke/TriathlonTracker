using Microsoft.EntityFrameworkCore;
using TriathlonTracker.Data;
using TriathlonTracker.Services;

namespace TriathlonTracker
{
    /// <summary>
    /// Helper class to seed configuration values securely.
    /// Run this once to set your actual Google OAuth credentials.
    /// </summary>
    public class ConfigurationSeeder
    {
        /// <summary>
        /// Seeds Google OAuth credentials from environment variables.
        /// Environment variables: GOOGLE_CLIENT_ID, GOOGLE_CLIENT_SECRET
        /// </summary>
        public static async Task SeedGoogleCredentialsFromEnvironment(string connectionString)
        {
            var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                Console.WriteLine("Warning: GOOGLE_CLIENT_ID or GOOGLE_CLIENT_SECRET environment variables not set.");
                Console.WriteLine("Google OAuth will not work until these are configured.");
                return;
            }

            await SeedGoogleCredentials(connectionString, clientId, clientSecret);
        }

        /// <summary>
        /// Seeds Google OAuth credentials with provided values.
        /// </summary>
        public static async Task SeedGoogleCredentials(string connectionString, string clientId, string clientSecret)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new ApplicationDbContext(optionsBuilder.Options);
            var encryptionService = new SimpleEncryptionService();
            var configService = new DatabaseConfigurationService(context, encryptionService);

            await configService.SetValueAsync(
                "Authentication:Google:ClientId", 
                clientId,
                "Google OAuth Client ID for authentication"
            );

            await configService.SetValueAsync(
                "Authentication:Google:ClientSecret", 
                clientSecret,
                "Google OAuth Client Secret for authentication"
            );

            Console.WriteLine("Google OAuth credentials have been securely stored in the database.");
        }

        /// <summary>
        /// Seeds JWT configuration from environment variables.
        /// Environment variables: JWT_KEY, JWT_ISSUER, JWT_AUDIENCE
        /// </summary>
        public static async Task SeedJwtConfigurationFromEnvironment(string connectionString)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

            if (string.IsNullOrEmpty(jwtKey))
            {
                Console.WriteLine("Warning: JWT_KEY environment variable not set. Using default key.");
                jwtKey = GenerateSecureJwtKey();
            }

            if (string.IsNullOrEmpty(jwtIssuer))
            {
                jwtIssuer = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" 
                    ? "https://yourdomain.com" 
                    : "https://localhost:7193";
            }

            if (string.IsNullOrEmpty(jwtAudience))
            {
                jwtAudience = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production" 
                    ? "https://yourdomain.com" 
                    : "https://localhost:7193";
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using var context = new ApplicationDbContext(optionsBuilder.Options);
            var encryptionService = new SimpleEncryptionService();
            var configService = new DatabaseConfigurationService(context, encryptionService);

            await configService.SetValueAsync("Jwt:Key", jwtKey, "JWT signing key");
            await configService.SetValueAsync("Jwt:Issuer", jwtIssuer, "JWT issuer");
            await configService.SetValueAsync("Jwt:Audience", jwtAudience, "JWT audience");

            Console.WriteLine("JWT configuration has been securely stored in the database.");
        }

        /// <summary>
        /// Generates a secure random JWT key.
        /// </summary>
        private static string GenerateSecureJwtKey()
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[64]; // 512 bits
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }

    // Simple encryption service for seeding
    internal class SimpleEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private const string EncryptedPrefix = "ENC:";

        public SimpleEncryptionService()
        {
            var keyString = Environment.GetEnvironmentVariable("ENCRYPTION_KEY") ?? "TriathlonTracker2024SecretKey32";
            _key = System.Text.Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
            
            var ivString = Environment.GetEnvironmentVariable("ENCRYPTION_IV") ?? "TriathlonIV16Bit";
            _iv = System.Text.Encoding.UTF8.GetBytes(ivString.PadRight(16).Substring(0, 16));
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText) || IsEncrypted(plainText))
                return plainText;

            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new System.Security.Cryptography.CryptoStream(msEncrypt, encryptor, System.Security.Cryptography.CryptoStreamMode.Write);
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

                using var aes = System.Security.Cryptography.Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                using var decryptor = aes.CreateDecryptor();
                using var msDecrypt = new MemoryStream(encryptedData);
                using var csDecrypt = new System.Security.Cryptography.CryptoStream(msDecrypt, decryptor, System.Security.Cryptography.CryptoStreamMode.Read);
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
}