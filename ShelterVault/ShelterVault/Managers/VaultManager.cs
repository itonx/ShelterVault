using System;
using System.Collections.Generic;
using ShelterVault.DataLayer;
using ShelterVault.Interfaces;
using ShelterVault.Models;
using ShelterVault.Shared.Extensions;

namespace ShelterVault.Managers
{
    public class VaultManager : IVaultManager
    {
        private readonly IEncryptionServiceFactory _encryptionServiceFactory;
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultCredentials _shelterVaultCredentials;

        public VaultManager(
            IEncryptionServiceFactory encryptionServiceFactory,
            IShelterVault shelterVault,
            IShelterVaultCredentials shelterVaultCredentials
        )
        {
            _encryptionServiceFactory = encryptionServiceFactory;
            _shelterVault = shelterVault;
            _shelterVaultCredentials = shelterVaultCredentials;
        }

        public IList<VaultModel> GetCurrentVaultWithCredentials()
        {
            List<VaultModel> vaults = new List<VaultModel>();
            ShelterVaultModel vault = _shelterVault.GetCurrentVault();
            IEnumerable<ShelterVaultCredentialsModel> credentials =
                _shelterVaultCredentials.GetAllCredentials(vault.UUID);
            VaultModel vaultModel = new(vault, credentials);
            vaults.Add(vaultModel);

            return vaults;
        }

        public bool IsValid(
            string masterKey,
            ShelterVaultModel shelterVaultModel,
            out byte[] encryptionKey
        )
        {
            var encryptionService = _encryptionServiceFactory.Create(
                (int)shelterVaultModel.Version
            );

            byte[] derivedKey = encryptionService.DeriveKeyFromPassword(
                masterKey,
                shelterVaultModel.Salt.FromBase64ToBytes()
            );

            if (shelterVaultModel.Version == (int)EncryptionVersion.v1)
            {
                string expectedValue = encryptionService.DecryptAes(shelterVaultModel, derivedKey);
                encryptionKey = new byte[] { 0x00 };
                return expectedValue != null && expectedValue.Equals(shelterVaultModel.UUID);
            }
            else if (shelterVaultModel.Version == (int)EncryptionVersion.v2)
            {
                byte[] encryptionKeyTmp = encryptionService.DecryptAesBytes(
                    shelterVaultModel.EncryptedTestValue.FromBase64ToBytes(),
                    derivedKey,
                    shelterVaultModel.Iv.FromBase64ToBytes()
                );
                encryptionKey = encryptionKeyTmp;
                return encryptionKeyTmp != null && encryptionKeyTmp.Length == 32;
            }

            throw new ArgumentNullException("Version");
        }
    }
}
