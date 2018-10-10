using System;
using System.Runtime.InteropServices;
using System.Security;

namespace FacebookFriendFeedDelimiter
{
    internal static class StringHelper
    {
        private static readonly byte[] Entropy = System.Text.Encoding.Unicode.GetBytes(Guid.NewGuid().ToString());

        public static string EncryptString(this SecureString input)
        {
            var encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(input.ConvertToInsecureString()),
                Entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        public static SecureString DecryptString(this string encryptedData)
        {
            try
            {
                var decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    Entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        public static SecureString ToSecureString(this string input)
        {
            var secure = new SecureString();
            foreach (var c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        public static string ConvertToInsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException(nameof(securePassword));
            var managedString = IntPtr.Zero;
            try
            {
                managedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(managedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(managedString);
            }
        }
    }
}