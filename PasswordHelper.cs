using System;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp4
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(password, 16, 100_000, HashAlgorithmName.SHA256);
            var salt = deriveBytes.Salt;
            var key = deriveBytes.GetBytes(32);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(key)}";
        }

        public static bool VerifyPassword(string password, string hash)
        {
            var parts = hash.Split(':');
            if (parts.Length != 2) return false;
            var salt = Convert.FromBase64String(parts[0]);
            var key = Convert.FromBase64String(parts[1]);
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var testKey = deriveBytes.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(testKey, key);
        }
    }
}
