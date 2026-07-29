using System.Security.Cryptography;
using System.Text;
using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;

namespace ShelterVault.Managers
{
    public interface IVaultCreatorManager
    {
        bool CreateVault(string uuid, string name, string masterKey);
    }

    public class VaultCreatorManager : IVaultCreatorManager
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultLocalDb _shelterVaultLocalDb;
        private readonly ICloudSyncManager _cloudSyncManager;

        public VaultCreatorManager(
            IEncryptionService encryptionService,
            IShelterVault shelterVault,
            ICloudSyncManager cloudSyncManager,
            IShelterVaultLocalDb shelterVaultLocalDb,
            IShelterVaultCredentials shelterVaultCredentials
        )
        {
            _encryptionService = encryptionService;
            _shelterVault = shelterVault;
            _cloudSyncManager = cloudSyncManager;
            _shelterVaultLocalDb = shelterVaultLocalDb;
        }

        public bool CreateVault(string uuid, string name, string masterKey)
        {
            try
            {
                byte[] saltBytes = RandomNumberGenerator.GetBytes(16);
                byte[] derivedKey = _encryptionService.DeriveKeyFromPassword(masterKey, saltBytes);

                (byte[] encryptedTestValue, byte[] iv) = _encryptionService.EncryptAes(
                    uuid,
                    derivedKey
                );
                _shelterVaultLocalDb.SetDbName(name);
                bool vaultCreated = _shelterVault.CreateShelterVault(
                    uuid,
                    name,
                    encryptedTestValue.ToBase64(),
                    iv.ToBase64(),
                    saltBytes.ToBase64(),
                    1
                );
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
    }
}
