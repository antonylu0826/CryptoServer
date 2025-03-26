using System.Security.Cryptography;
using System.Text;

namespace CryptoServer.Services;

public interface IEncryptionService
{
    string Encrypt(string text);
    string Decrypt(string encryptedText);
}

public class EncryptionService : IEncryptionService
{
    private readonly string _key;
    private readonly byte[] _keyBytes;
    private readonly byte[] _iv;

    public EncryptionService(string key)
    {
        _key = key;
        using var sha256 = SHA256.Create();
        _keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(_key));
        _iv = new byte[16];
        Array.Copy(_keyBytes, _iv, 16);
    }

    public string Encrypt(string text)
    {
        using var aes = Aes.Create();
        aes.Key = _keyBytes;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        var textBytes = Encoding.UTF8.GetBytes(text);
        var encryptedBytes = encryptor.TransformFinalBlock(textBytes, 0, textBytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string encryptedText)
    {
        using var aes = Aes.Create();
        aes.Key = _keyBytes;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
} 