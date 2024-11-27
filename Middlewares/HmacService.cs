using Cgmail.Common.Model;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Cgmail.Common.Middlewares;

public interface IHmacService
{
    T? DecryptAndDeserialize<T>(string encryptedData, string hmac, string secretKey, string iv);

    SecureRequest EncryptData<T>(T? plainText, string secretKey);

    SecureRequest ParseEncryptedRequest(string encryptedRequest);

}
public class HmacService : IHmacService
{
    public HmacService()
    {
    }

    public T? DecryptAndDeserialize<T>(string encryptedData, string hmac, string secretKey, string iv)
    {
        if (!ValidateHMAC(encryptedData, hmac, secretKey))
        {
            throw new UnauthorizedAccessException("Invalid HMAC");
        }

        var decryptedPayload = Decrypt(encryptedData, secretKey, iv);

        T? result = JsonConvert.DeserializeObject<T>(decryptedPayload);

        return result;
    }

    public SecureRequest EncryptData<T>(T? plainText, string secretKey)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(secretKey);
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainTextString = JsonConvert.SerializeObject(plainText);
            var plainBytes = Encoding.UTF8.GetBytes(plainTextString);

            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            var encryptedData = Convert.ToBase64String(encryptedBytes);
            var iv = Convert.ToBase64String(aes.IV);

            var hmac = GenerateHmac(encryptedData, secretKey, iv);

            return new SecureRequest
            {
                EncryptedData = encryptedData,
                Iv = iv,
                Hmac = hmac
            };
        }
    }

    private static bool ValidateHMAC(string encryptedData, string receivedHmac, string secretKey)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            var computedHmac = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(encryptedData)));
            return computedHmac == receivedHmac;
        }
    }

    private static string Decrypt(string encryptedData, string secretKey, string iv)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = GetValidKey(secretKey);
            aes.IV = Convert.FromBase64String(iv);
            aes.Padding = PaddingMode.PKCS7;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                var cipherBytes = Convert.FromBase64String(encryptedData);
                var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return Encoding.UTF8.GetString(plainBytes);
            }
        }
    }

    private static byte[] GetValidKey(string secretKey)
    {
        return Encoding.UTF8.GetBytes(secretKey);
    }

    private static string GenerateHmac(string encryptedData, string secretKey, string iv)
    {
        var combinedData = $"{encryptedData}:{iv}";
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(combinedData));
            return Convert.ToBase64String(hash);

        }
    }

    public SecureRequest ParseEncryptedRequest(string encryptedRequest)
    {
        var splitRequest = encryptedRequest.Split("(/=/)");
        return new SecureRequest
        {
            EncryptedData = splitRequest[0],
            Iv = splitRequest[1],
            Hmac = splitRequest[2]
        };
    }

}