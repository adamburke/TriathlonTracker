namespace TriathlonTracker.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        bool IsEncrypted(string value);
    }
}