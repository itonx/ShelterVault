using System;
using System.IO;
using System.Security.Cryptography;

namespace ShelterVault.Tools
{
    public static class EncryptionTool
    {
        public static (byte[], byte[]) EncryptAes(ref string plainText, byte[] key)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));

            byte[] encrypted;
            byte[] lastIV;
            using (Aes aesAlg = Aes.Create())
            {
                byte[] finalKey = new byte[aesAlg.Key.Length];
                for(int i = 0; i < key.Length; i++)
                {
                    finalKey[i] = key[i];
                }
                aesAlg.Key = finalKey;
                lastIV = aesAlg.IV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return (encrypted, lastIV);
        }

        public static string DecryptAes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                byte[] finalKey = new byte[aesAlg.Key.Length];
                for (int i = 0; i < key.Length; i++)
                {
                    finalKey[i] = key[i];
                }
                aesAlg.Key = finalKey;
                aesAlg.IV = iv;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}
