using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;
using System.Collections.Generic;

namespace ShelterVault.Managers
{
    public interface IVaultManager
    {
        IList<VaultModel> GetCurrentVaultWithCredentials();
        bool IsValid(string masterKey, ShelterVaultModel shelterVaultModel);
    }

    public class VaultManager : IVaultManager
    {

        private readonly IEncryptionService _encryptionService;
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultCredentials _shelterVaultCredentials;

        public VaultManager(IEncryptionService encryptionService, IShelterVault shelterVault, IShelterVaultCredentials shelterVaultCredentials)
        {
            _encryptionService = encryptionService;
            _shelterVault = shelterVault;
            _shelterVaultCredentials = shelterVaultCredentials;
        }

        public IList<VaultModel> GetCurrentVaultWithCredentials()
        {
            List<VaultModel> vaults = new List<VaultModel>();
            ShelterVaultModel vault = _shelterVault.GetCurrentVault();
            IEnumerable<ShelterVaultCredentialsModel> credentials = _shelterVaultCredentials.GetAllCredentials(vault.UUID);
            VaultModel vaultModel = new(vault, credentials);
            vaults.Add(vaultModel);

            return vaults;
        }

        public bool IsValid(string masterKey, ShelterVaultModel shelterVaultModel)
        {
            byte[] derivedKey = _encryptionService.DeriveKeyFromPassword(masterKey, shelterVaultModel.Salt.FromBase64ToBytes());
            string expectedValue = _encryptionService.DecryptAes(shelterVaultModel, derivedKey);

            return expectedValue != null && expectedValue.Equals(shelterVaultModel.UUID);
        }
    }
}
