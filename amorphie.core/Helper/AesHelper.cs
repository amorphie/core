using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace amorphie.core.Helper;
public static class AesHelper
{

    private const string encryptPrefix = "$encrypt_";

    public static string EncryptString(string key, string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            var iv = new byte[16];
            aes.Key = CreateAesKeyAsByte(key);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor();

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write);
            using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            var array = memoryStream.ToArray();

            return encryptPrefix + Convert.ToBase64String(array) + "_" + Convert.ToBase64String(aes.IV);
        }
    }

    public static string DecryptString(string key, string cipherText)
    {
        if (!cipherText.StartsWith(encryptPrefix))
        {
            return cipherText;
        }
        var cipherParts = cipherText.Split('_');
        if (cipherParts.Length != 3)
        {
            return string.Empty;
        }
        byte[] cipherTextBuffer = Convert.FromBase64String(cipherParts[1]);
        byte[] iv = Convert.FromBase64String(cipherParts[2]);

        using (Aes aes = Aes.Create())
        {
            aes.Key = CreateAesKeyAsByte(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new MemoryStream(cipherTextBuffer);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using (StreamReader streamReader = new StreamReader(cryptoStream))
                return streamReader.ReadToEnd();
        }
    }
    public static string CreateAesKey(string key)
    {
        byte[] bytes = CreateAesKeyAsByte(key);
        return Encoding.ASCII.GetString(bytes);
    }

    public static byte[] CreateAesKeyAsByte(string key)
    {
        var keyByte = Encoding.UTF8.GetBytes(key);
        byte[] bytes = new byte[16];
        Array.Copy(keyByte, bytes, 16);
        return bytes;
    }


    public static JsonObject DecryptJsonToNewObject(string aesKey, JsonObject data)
    {
        if (data.DeepClone() is JsonObject newData)
        {
            return DecryptJson(aesKey, newData);
        }
        return data;
    }

    public static JsonObject DecryptJson(string aesKey, JsonObject data)
    {
        var dataKeys = data.Where(p => p.Value != null).Select(p => p.Key).ToList();
        foreach (var dataKey in dataKeys)
        {
            if (data[dataKey]!.GetValueKind() == JsonValueKind.Object)
            {
                if (data[dataKey] is JsonObject innerDict)
                {
                    var decResult = DecryptJson(aesKey, innerDict);
                    data[dataKey] = decResult as JsonObject;
                }
            }
            else if (data[dataKey]!.GetValueKind() == JsonValueKind.String)
            {
                data[dataKey] = DecryptString(aesKey, data[dataKey]!.ToString());
            }
        }
        return data;
    }
}

