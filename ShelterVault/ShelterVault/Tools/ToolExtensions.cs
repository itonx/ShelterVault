using System;
using System.Security.Cryptography;
using System.Text;

namespace ShelterVault.Tools
{
    public static class ToolExtensions
    {
        public static string ToSHA256Base64(this string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashValueBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(value));
                return Convert.ToBase64String(hashValueBytes);
            }
        }
    }
}
