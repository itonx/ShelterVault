using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;

namespace ShelterVault.Shared.Extensions
{
    public static class ShelterVaultExtensions
    {
        private static string _passwordPattern = @"^(?=.*\d)(?=.*[!@#$%^&*()-_=+[\]{};:'"",.<>/?])(?=.*[a-z])(?=.*[A-Z]).{8,32}$";

        public static string ToSHA256Hex(this string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToHexString(sha256.ComputeHash(value.GetBytes()));
            }
        }

        public static string ToBase64(this byte[] value)
        {
            return Convert.ToBase64String(value);
        }

        public static byte[] FromBase64ToBytes(this string base64Value)
        {
            return Convert.FromBase64String(base64Value);
        }

        public static byte[] GetBytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        public static string GetString(this byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }

        public static bool IsStrongPassword(this string value) => Regex.IsMatch(value, _passwordPattern);
        public static bool Has8Characters(this string value) => !string.IsNullOrWhiteSpace(value) && value.Length > 7;
        public static bool HasLowercase(this string value) => !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^(?=.*[a-z]).{1,}$");
        public static bool HasUppercase(this string value) => !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^(?=.*[A-Z]).{1,}$");
        public static bool HasNumber(this string value) => !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^(?=.*\d).{1,}$");
        public static bool HasSpecialChars(this string value) => !string.IsNullOrWhiteSpace(value) && Regex.IsMatch(value, @"^(?=.*[!@#$%^&*()\-_=+[\]{};:'"",.<>/?]).{1,}$");
        public static bool IsLessThan32(this string value) => !string.IsNullOrWhiteSpace(value) && value.Length < 33;

        public static void SendToClipboard(this string value)
        {
            DataPackage data = new();
            data.RequestedOperation = DataPackageOperation.Copy;
            data.SetText(value);
            Clipboard.SetContent(data);
        }

        public static IEnumerable<string> GetFilesByExtension(this string path, string extension)
        {
            return Directory
                .EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => extension.Equals(Path.GetExtension(s).TrimStart('.'), StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
