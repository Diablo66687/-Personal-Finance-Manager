using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp4
{
    public static class AesEncryptionHelper
    {
        private static byte[] GetKey()
        {
            var keyStr = Environment.GetEnvironmentVariable("FINANCE_AES_KEY");
            if (string.IsNullOrEmpty(keyStr) || keyStr.Length != 32)
                throw new Exception("AES klíè musí být 32 znakù a nastaven v promìnné FINANCE_AES_KEY.");
            return Encoding.UTF8.GetBytes(keyStr);
        }
        private static byte[] GetIV()
        {
            var ivStr = Environment.GetEnvironmentVariable("FINANCE_AES_IV");
            if (string.IsNullOrEmpty(ivStr) || ivStr.Length != 16)
                throw new Exception("AES IV musí být 16 znakù a nastaven v promìnné FINANCE_AES_IV.");
            return Encoding.UTF8.GetBytes(ivStr);
        }
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            using var aes = Aes.Create();
            aes.Key = GetKey();
            aes.IV = GetIV();
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";
            using var aes = Aes.Create();
            aes.Key = GetKey();
            aes.IV = GetIV();
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}
