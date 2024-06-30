using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ShelterVault.Tools
{
    public static class ToolExtensions
    {
        private static readonly Regex _passwordChecker = new Regex(@"^(?=.*[0-9])(?=.*[!@#$%^&*(),.?"":{}|<>])[a-zA-Z0-9!@#$%^&*(),.?"":{}|<>]{8,32}$");

        public static string ToSHA256Base64(this string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashValueBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(value));
                return Convert.ToBase64String(hashValueBytes);
            }
        }

        public static bool IsStrongPassword(this string value) => _passwordChecker.IsMatch(value);
    }
}
