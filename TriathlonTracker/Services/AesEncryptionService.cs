using System.Security.Cryptography;
using System.Text;

namespace TriathlonTracker.Services
{
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private const string EncryptedPrefix = "ENC:";

        public AesEncryptionService()
        {
            var keyString = Environment.GetEnvironmentVariable("ENCRYPTION_KEY") ?? "TriathlonTracker2024SecretKey32";
            _key = System.Text.Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));

            var ivString = Environment.GetEnvironmentVariable("ENCRYPTION_IV") ?? "TriathlonIV16Bit";
            _iv = System.Text.Encoding.UTF8.GetBytes(ivString.PadRight(16).Substring(0, 16));
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            if (IsEncrypted(plainText))
                return plainText; // Already encrypted

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
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            if (!IsEncrypted(cipherText))
                return cipherText; // Not encrypted, return as-is

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
                // If decryption fails, return the original value
                return cipherText;
            }
        }

        public bool IsEncrypted(string value)
        {
            return !string.IsNullOrEmpty(value) && value.StartsWith(EncryptedPrefix);
        }
    }
}