using ShelterVault.Models;
using ShelterVault.Shared.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;

namespace ShelterVault.Services
{
    public interface IEncryptionService
    {
        (byte[], byte[]) EncryptAes(string plainText, byte[] key, byte[] salt);
        string DecryptAes(byte[] cipherText, byte[] key, byte[] iv, byte[] salt);
        string DecryptAes(ShelterVaultCredentialsModel shelterVaultCredentialsModel, byte[] key, byte[] salt);
        string DecryptAes(ShelterVaultModel shelterVaultModel, byte[] key);
        string DecryptAes(CredentialsViewItem credentialsViewItem, byte[] key, byte[] salt);
        string DecryptAes(ShelterVaultCloudConfigModel shelterVaultCloudConfigModel, byte[] key, byte[] salt);
        byte[] DeriveKeyFromPassword(string password, byte[] salt, int keyLength = 32);
    }

    public class EncryptionService : IEncryptionService
    {
        public (byte[], byte[]) EncryptAes(string plainText, byte[] key, byte[] salt)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length == 0)
                throw new ArgumentNullException(nameof(key));

            byte[] encrypted;
            byte[] lastIV;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
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

        public string DecryptAes(ShelterVaultCredentialsModel shelterVaultCredentialsModel, byte[] key, byte[] salt)
        {
            return DecryptAes(shelterVaultCredentialsModel.EncryptedValues.FromBase64ToBytes(), key, shelterVaultCredentialsModel.Iv.FromBase64ToBytes(), salt);
        }

        public string DecryptAes(ShelterVaultModel shelterVaultModel, byte[] key)
        {
            return DecryptAes(shelterVaultModel.EncryptedTestValue.FromBase64ToBytes(), key, shelterVaultModel.Iv.FromBase64ToBytes(), shelterVaultModel.Salt.FromBase64ToBytes());
        }

        public string DecryptAes(CredentialsViewItem credentialsViewItem, byte[] key, byte[] salt)
        {
            return DecryptAes(credentialsViewItem.EncryptedValues.FromBase64ToBytes(), key, credentialsViewItem.Iv.FromBase64ToBytes(), salt);
        }

        public string DecryptAes(ShelterVaultCloudConfigModel shelterVaultCloudConfigModel, byte[] key, byte[] salt)
        {
            return DecryptAes(shelterVaultCloudConfigModel.EncryptedValues.FromBase64ToBytes(), key, shelterVaultCloudConfigModel.Iv.FromBase64ToBytes(), salt);
        }

        public string DecryptAes(byte[] cipherText, byte[] key, byte[] iv, byte[] salt)
        {
            try
            {
                if (cipherText == null || cipherText.Length <= 0)
                    throw new ArgumentNullException(nameof(cipherText));
                if (key == null || key.Length <= 0)
                    throw new ArgumentNullException(nameof(key));

                string plaintext = null;

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
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
            catch (Exception)
            {
                return null;
            }

        }

        public byte[] DeriveKeyFromPassword(string password, byte[] salt, int keyLength = 32)
        {
            using (var keyDerivationFunction = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                return keyDerivationFunction.GetBytes(keyLength);
            }
        }
    }
}
