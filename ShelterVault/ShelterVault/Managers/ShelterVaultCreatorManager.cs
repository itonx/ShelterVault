using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Managers
{
    internal interface IShelterVaultCreatorManager
    {
        bool CreateVault(string uuid, string name, string masterKey, string salt);
    }

    internal class ShelterVaultCreatorManager : IShelterVaultCreatorManager
    {

        private readonly IEncryptionService _encryptionService;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly ICloudSyncManager _cloudSyncManager;

        public ShelterVaultCreatorManager(IEncryptionService encryptionService, IShelterVaultLocalStorage shelterVaultLocalStorage, ICloudSyncManager cloudSyncManager)
        {
            _encryptionService = encryptionService;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _cloudSyncManager = cloudSyncManager;
        }

        public bool CreateVault(string uuid, string name, string masterKey, string salt)
        {
            try
            {
                byte[] masterKeyBytes = masterKey.GetBytes();
                string masterKeyHash = masterKey.ToSHA256Hex();
                byte[] saltBytes = salt.GetBytes();

                (byte[] encryptedMasterKeyHash, byte[] iv) = _encryptionService.EncryptAes(masterKeyHash, masterKeyBytes, saltBytes);

                bool vaultCreated = _shelterVaultLocalStorage.CreateShelterVault(uuid, name, encryptedMasterKeyHash.ToBase64(), iv.ToBase64(), saltBytes.ToBase64());
                if (vaultCreated)
                {
                    ShelterVaultModel vault = _shelterVaultLocalStorage.GetVaultByUUID(uuid);
                    _cloudSyncManager.UpsertItemAsync(vault);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
