using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShelterVault.Managers
{
    public interface IVaultManager
    {
        bool CreateVault(string uuid, string name, string masterKey, string salt);
        IList<VaultModel> GetCurrentVaultWithCredentials();
        bool IsValid(string masterKey, ShelterVaultModel shelterVaultModel);
        void DeleteMany(IEnumerable<CosmosDBSyncModel> synchronizedModels);
    }

    public class VaultManager : IVaultManager
    {

        private readonly IEncryptionService _encryptionService;
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultLocalDb _shelterVaultLocalDb;
        private readonly IShelterVaultCredentials _shelterVaultCredentials;
        private readonly ICloudSyncManager _cloudSyncManager;

        public VaultManager(IEncryptionService encryptionService, IShelterVault shelterVault, ICloudSyncManager cloudSyncManager, IShelterVaultLocalDb shelterVaultLocalDb, IShelterVaultCredentials shelterVaultCredentials)
        {
            _encryptionService = encryptionService;
            _shelterVault = shelterVault;
            _cloudSyncManager = cloudSyncManager;
            _shelterVaultLocalDb = shelterVaultLocalDb;
            _shelterVaultCredentials = shelterVaultCredentials;
        }

        public bool CreateVault(string uuid, string name, string masterKey, string salt)
        {
            try
            {
                byte[] masterKeyBytes = masterKey.GetBytes();
                string masterKeyHash = masterKey.ToSHA256Hex();
                byte[] saltBytes = salt.GetBytes();

                (byte[] encryptedMasterKeyHash, byte[] iv) = _encryptionService.EncryptAes(masterKeyHash, masterKeyBytes, saltBytes);
                _shelterVaultLocalDb.SetDbName(name);
                bool vaultCreated = _shelterVault.CreateShelterVault(uuid, name, encryptedMasterKeyHash.ToBase64(), iv.ToBase64(), saltBytes.ToBase64(), 1);
                if (vaultCreated)
                {
                    ShelterVaultModel vault = _shelterVault.GetVaultByUUID(uuid);
                    _cloudSyncManager.UpsertItemAsync(vault);
                }
            }
            catch
            {
                return false;
            }

            return true;
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
            byte[] masterKeyBytes = masterKey.GetBytes();
            string expectedMasterKeyHash = _encryptionService.DecryptAes(shelterVaultModel, masterKeyBytes);

            return expectedMasterKeyHash != null && expectedMasterKeyHash.Equals(masterKey.ToSHA256Hex());
        }

        public void DeleteMany(IEnumerable<CosmosDBSyncModel> synchronizedModels)
        {
            foreach (string id in synchronizedModels.Where(x => x.source == SourceType.Local && x.version == -1 && x.type.Equals(ShelterVaultConstants.PARTITION_SHELTER_VAULT)).Select(x => x.id))
                _shelterVault.DeleteVault(id);
        }
    }
}
