using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShelterVault.DataLayer;
using ShelterVault.Interfaces;
using ShelterVault.Models;

namespace ShelterVault.Services
{
    public class ShelterVaultEncryption : IShelterVaultEncryption
    {
        private readonly IEncryptionServiceFactory _encryptionServiceFactory;
        private readonly IShelterVault _shelterVault;

        public ShelterVaultEncryption(
            IEncryptionServiceFactory encryptionServiceFactory,
            IShelterVault shelterVault
        )
        {
            _encryptionServiceFactory = encryptionServiceFactory;
            _shelterVault = shelterVault;
        }

        public string DecryptAes(byte[] cipherText, byte[] key, byte[] iv)
        {
            var encryptionService = GetEncryptionService();
            return encryptionService.DecryptAes(cipherText, key, iv);
        }

        public string DecryptAes(
            ShelterVaultCredentialsModel shelterVaultCredentialsModel,
            byte[] key
        )
        {
            var encryptionService = GetEncryptionService();
            return encryptionService.DecryptAes(shelterVaultCredentialsModel, key);
        }

        public string DecryptAes(ShelterVaultModel shelterVaultModel, byte[] key)
        {
            var encryptionService = GetEncryptionService();
            return encryptionService.DecryptAes(shelterVaultModel, key);
        }

        public string DecryptAes(CredentialsViewItem credentialsViewItem, byte[] key)
        {
            var encryptionService = GetEncryptionService();
            return encryptionService.DecryptAes(credentialsViewItem, key);
        }

        public string DecryptAes(
            ShelterVaultCloudConfigModel shelterVaultCloudConfigModel,
            byte[] key
        )
        {
            var encryptionService = GetEncryptionService();
            return encryptionService.DecryptAes(shelterVaultCloudConfigModel, key);
        }

        public byte[] DecryptAesBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            var encryptionService = GetEncryptionService();
            return encryptionService.DecryptAesBytes(cipherText, key, iv);
        }

        public byte[] DeriveKeyFromPassword(string password, byte[] salt)
        {
            var encryptionService = GetEncryptionService();
            return encryptionService.DeriveKeyFromPassword(password, salt);
        }

        public (byte[], byte[]) EncryptAes(string plainText, byte[] key)
        {
            var encryptionService = GetEncryptionService();
            return encryptionService.EncryptAes(plainText, key);
        }

        public (byte[], byte[]) EncryptAes(byte[] unencryptedBytes, byte[] key)
        {
            var encryptionService = GetEncryptionService();
            return encryptionService.EncryptAes(unencryptedBytes, key);
        }

        public (byte[], byte[]) EncryptAes(string plainText, byte[] key, int? version)
        {
            var encryptionService = GetEncryptionService(version);
            return encryptionService.EncryptAes(plainText, key);
        }

        private IEncryptionService GetEncryptionService(int? version = null)
        {
            if (version == null)
            {
                var vault = _shelterVault.GetCurrentVault();
                return _encryptionServiceFactory.Create((int)vault.Version);
            }

            return _encryptionServiceFactory.Create((int)version);
        }
    }
}
