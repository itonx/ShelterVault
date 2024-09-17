using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;

namespace ShelterVault.Tools
{
    public static class ToolExtensions
    {
        private static string _passwordPattern = @"^(?=.*\d)(?=.*[!@#$%^&*()-_=+[\]{};:'"",.<>/?])(?=.*[a-z])(?=.*[A-Z]).{8,32}$";

        public static string ToSHA256Base64(this string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashValueBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                return Convert.ToBase64String(hashValueBytes);
            }
        }

        public static string ToBase64Encoded(this string value)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(valueBytes);
        }

        public static string ToBase64Decoded(this string base64Value)
        {
            byte[] base64Bytes = Convert.FromBase64String(base64Value);
            return Encoding.UTF8.GetString(base64Bytes);
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
    }
}
