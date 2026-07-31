using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ShelterVault.Interfaces;
using ShelterVault.Models;
using ShelterVault.Shared.Extensions;
using Windows.Storage.Streams;

namespace ShelterVault.Services
{
    public class EncryptionServiceV0 : IEncryptionService
    {
        private readonly PBKDF2KeyDerivation _pbkdf2KeyDerivation = new();

        public (byte[], byte[]) EncryptAes(string plainText, byte[] key)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            return EncryptAes(plainBytes, key);
        }

        public string DecryptAes(
            ShelterVaultCredentialsModel shelterVaultCredentialsModel,
            byte[] key
        )
        {
            return DecryptAes(
                shelterVaultCredentialsModel.EncryptedValues.FromBase64ToBytes(),
                key,
                shelterVaultCredentialsModel.Iv.FromBase64ToBytes()
            );
        }

        public string DecryptAes(ShelterVaultModel shelterVaultModel, byte[] key)
        {
            return DecryptAes(
                shelterVaultModel.EncryptedTestValue.FromBase64ToBytes(),
                key,
                shelterVaultModel.Iv.FromBase64ToBytes()
            );
        }

        public string DecryptAes(CredentialsViewItem credentialsViewItem, byte[] key)
        {
            return DecryptAes(
                credentialsViewItem.EncryptedValues.FromBase64ToBytes(),
                key,
                credentialsViewItem.Iv.FromBase64ToBytes()
            );
        }

        public string DecryptAes(
            ShelterVaultCloudConfigModel shelterVaultCloudConfigModel,
            byte[] key
        )
        {
            return DecryptAes(
                shelterVaultCloudConfigModel.EncryptedValues.FromBase64ToBytes(),
                key,
                shelterVaultCloudConfigModel.Iv.FromBase64ToBytes()
            );
        }

        public string DecryptAes(byte[] cipherText, byte[] key, byte[] iv)
        {
            return Encoding.UTF8.GetString(DecryptAesBytes(cipherText, key, iv));
        }

        public byte[] DecryptAesBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            try
            {
                if (cipherText == null || cipherText.Length <= 0)
                    throw new ArgumentNullException(nameof(cipherText));
                if (key == null || key.Length <= 0)
                    throw new ArgumentNullException(nameof(key));

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                    {
                        using (
                            CryptoStream csDecrypt = new CryptoStream(
                                msDecrypt,
                                decryptor,
                                CryptoStreamMode.Read
                            )
                        )
                        {
                            using var outputStream = new MemoryStream();
                            csDecrypt.CopyTo(outputStream);
                            return outputStream.ToArray();
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public byte[] DeriveKeyFromPassword(string password, byte[] salt)
        {
            var kdo = new KeyDerivationOptions
            {
                Salt = salt,
                Iterations = 10000,
                KeyLength = 32,
                Algorithm = EncryptionVersion.v1,
            };
            return _pbkdf2KeyDerivation.DeriveKey(password, kdo);
        }

        public (byte[], byte[]) EncryptAes(byte[] unencryptedBytes, byte[] key)
        {
            if (unencryptedBytes == null || unencryptedBytes.Length == 0)
                throw new ArgumentNullException(nameof(unencryptedBytes));
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
                    using (
                        CryptoStream csEncrypt = new CryptoStream(
                            msEncrypt,
                            encryptor,
                            CryptoStreamMode.Write
                        )
                    )
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(unencryptedBytes);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return (encrypted, lastIV);
        }
    }
}
