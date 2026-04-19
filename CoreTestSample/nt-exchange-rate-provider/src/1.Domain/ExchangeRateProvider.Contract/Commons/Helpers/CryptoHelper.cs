using System.Security.Cryptography;
using System.Text;

namespace ExchangeRateProvider.Contract.Commons.Helpers;

public static class CryptoHelper
{
    public static string EncryptString(this string message, string encryptionKey)
    {
        byte[] results;
        UTF8Encoding utf8 = new UTF8Encoding();
        MD5CryptoServiceProvider hashProvider = new();
        byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(encryptionKey));

        TripleDESCryptoServiceProvider tdesAlgorithm = new()
        {
            Key = tdesKey,
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7
        };
        try
        {
            byte[] dataToEncrypt = utf8.GetBytes(message);

            ICryptoTransform encryptor = tdesAlgorithm.CreateEncryptor();
            results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
        }
        catch
        {
            return "";
        }
        finally
        {
            tdesAlgorithm.Clear();
            hashProvider.Clear();
        }
        return Convert.ToBase64String(results).Replace("/", "-").Replace("+", "_");
    }

    public static string DecryptString(this string message, string encryptionKey)
    {
        byte[] results;
        UTF8Encoding utf8 = new UTF8Encoding();
        MD5CryptoServiceProvider hashProvider = new();
        byte[] tdesKey = hashProvider.ComputeHash(utf8.GetBytes(encryptionKey));
        TripleDESCryptoServiceProvider tdesAlgorithm = new()
        {
            Key = tdesKey,
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7
        };
        try
        {
            byte[] dataToDecrypt = Convert.FromBase64String(message.Replace("-", "/").Replace("_", "+"));

            ICryptoTransform decryptor = tdesAlgorithm.CreateDecryptor();
            results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
        }
        catch
        {
            results = new byte[0];
            return "0";
        }
        finally
        {
            tdesAlgorithm.Clear();
            hashProvider.Clear();
        }

        return utf8.GetString(results);
    }

    public static byte[] Encrypt(string plainText, string key)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }

                    array = memoryStream.ToArray();
                }
            }
        }

        return array;
    }

    public static string Decrypt(byte[] cipherText, string key)
    {
        byte[] iv = new byte[16];
        byte[] buffer = cipherText;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}