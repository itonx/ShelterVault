using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Core;
using ShelterVault.Interfaces;
using ShelterVault.Models;
using ShelterVault.Shared.Extensions;

namespace ShelterVault.Services
{
    public class EncryptionServiceV1 : IEncryptionService
    {
        private readonly Argon2KeyDerivation _argon2KeyDerivationService = new();
        private const int NonceSizeBytes = 12;
        private const int TagSizeBytes = 16;

        public string DecryptAes(byte[] cipherText, byte[] key, byte[] iv)
        {
            return Encoding.UTF8.GetString(DecryptAesBytes(cipherText, key, iv));
        }

        public byte[] DecryptAesBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            try
            {
                if (cipherText == null || cipherText.Length == 0)
                    throw new ArgumentNullException(nameof(cipherText));
                if (key == null || key.Length == 0)
                    throw new ArgumentNullException(nameof(key));

                byte[] nonce = cipherText[..NonceSizeBytes];
                byte[] tag = cipherText[^TagSizeBytes..];
                byte[] ciphertext = cipherText[NonceSizeBytes..^TagSizeBytes];
                byte[] plaintextBytes = new byte[ciphertext.Length];

                using (var aesGcm = new AesGcm(key, TagSizeBytes))
                {
                    // Decrypt and verify tag
                    aesGcm.Decrypt(nonce, ciphertext, tag, plaintextBytes);
                }

                return plaintextBytes;
            }
            catch (Exception)
            {
                return null; //null here means "this data was tampered with or is not what it claims to be
            }
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

        public byte[] DeriveKeyFromPassword(string password, byte[] salt)
        {
            var kdo = new KeyDerivationOptions
            {
                Salt = salt,
                Iterations = 3,
                MemorySize = 64 * 1024,
                Parallelism = Environment.ProcessorCount,
                KeyLength = 32,
                Algorithm = EncryptionVersion.v2,
            };
            return _argon2KeyDerivationService.DeriveKey(password, kdo);
        }

        public (byte[], byte[]) EncryptAes(string plainText, byte[] key)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                throw new ArgumentNullException(nameof(plainText));
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            return EncryptAes(plainBytes, key);
        }

        private (byte[], byte[]) EncryptAes(byte[] unencryptedBytes, byte[] key)
        {
            if (unencryptedBytes == null || unencryptedBytes.Length == 0)
                throw new ArgumentNullException(nameof(unencryptedBytes));
            if (key == null || key.Length == 0)
                throw new ArgumentNullException(nameof(key));

            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes); // fresh random nonce every call - never reuse a nonce with the same key
            byte[] ciphertext = new byte[unencryptedBytes.Length];
            byte[] tag = new byte[TagSizeBytes];

            using (var aesGcm = new AesGcm(key, TagSizeBytes))
            {
                aesGcm.Encrypt(nonce, unencryptedBytes, ciphertext, tag);
            }

            // Combine: Nonce + Ciphertext + Tag
            byte[] result = new byte[NonceSizeBytes + ciphertext.Length + TagSizeBytes];
            Buffer.BlockCopy(nonce, 0, result, 0, NonceSizeBytes);
            Buffer.BlockCopy(ciphertext, 0, result, NonceSizeBytes, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, NonceSizeBytes + ciphertext.Length, TagSizeBytes);

            return (result, Encoding.UTF8.GetBytes("unusedvalueforaesgcm"));
        }

        public (byte[], byte[]) EncryptAes(string plainText, byte[] key, int? version)
        {
            throw new NotImplementedException();
        }
    }
}
