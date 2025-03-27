using System.Security.Cryptography;
using System.Text;

namespace CryptoServer.Services;

internal interface IEncryptionService
{
    string Encrypt(string text);
    string Decrypt(string encryptedText);
}

internal class EncryptionService : IEncryptionService
{
    private readonly string _key;
    private readonly byte[] _keyBytes;
    private readonly byte[] _iv;

    public EncryptionService(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length < 32)
        {
            throw new ArgumentException("Encryption key must be at least 32 characters long", nameof(key));
        }

        _key = key;
        _keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(_key));
        
        // 使用隨機 IV
        _iv = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(_iv);
    }

    public string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new ArgumentException("Text cannot be empty", nameof(text));
        }

        using var aes = Aes.Create();
        aes.Key = _keyBytes;
        aes.IV = _iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var textBytes = Encoding.UTF8.GetBytes(text);
        var encryptedBytes = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
        {
            throw new ArgumentException("Encrypted text cannot be empty", nameof(encryptedText));
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _keyBytes;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid encrypted text format", nameof(encryptedText));
        }
        catch (CryptographicException)
        {
            throw new ArgumentException("Failed to decrypt text", nameof(encryptedText));
        }
    }
} 