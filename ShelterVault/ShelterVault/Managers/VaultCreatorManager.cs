using System.Security.Cryptography;
using System.Text;
using ShelterVault.DataLayer;
using ShelterVault.Interfaces;
using ShelterVault.Models;
using ShelterVault.Shared.Extensions;

namespace ShelterVault.Managers
{
    public class VaultCreatorManager : IVaultCreatorManager
    {
        private readonly IEncryptionServiceFactory _encryptionServiceFactory;
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultLocalDb _shelterVaultLocalDb;
        private readonly ICloudSyncManager _cloudSyncManager;
        private readonly int DEFAULT_ENCRYPTION_VERSION = (int)EncryptionVersion.v2;

        public VaultCreatorManager(
            IEncryptionServiceFactory encryptionServiceFactory,
            IShelterVault shelterVault,
            ICloudSyncManager cloudSyncManager,
            IShelterVaultLocalDb shelterVaultLocalDb,
            IShelterVaultCredentials shelterVaultCredentials
        )
        {
            _encryptionServiceFactory = encryptionServiceFactory;
            _shelterVault = shelterVault;
            _cloudSyncManager = cloudSyncManager;
            _shelterVaultLocalDb = shelterVaultLocalDb;
        }

        public bool CreateVault(string uuid, string name, string masterKey)
        {
            try
            {
                var encryptionService = _encryptionServiceFactory.Create(
                    DEFAULT_ENCRYPTION_VERSION
                );

                byte[] saltBytes = RandomNumberGenerator.GetBytes(32);
                byte[] derivedKey = encryptionService.DeriveKeyFromPassword(masterKey, saltBytes);
                byte[] vaultKey = RandomNumberGenerator.GetBytes(32);

                (byte[] encryptedVaultKey, byte[] iv) = encryptionService.EncryptAes(
                    vaultKey,
                    derivedKey
                );
                _shelterVaultLocalDb.SetDbName(name);
                bool vaultCreated = _shelterVault.CreateShelterVault(
                    uuid,
                    name,
                    encryptedVaultKey.ToBase64(),
                    iv.ToBase64(),
                    saltBytes.ToBase64(),
                    DEFAULT_ENCRYPTION_VERSION
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

        public bool CreateVault(string uuid, string name, string masterKey, out byte[] vaultKey) //Argon
        {
            vaultKey = null;

            try
            {
                var encryptionService = _encryptionServiceFactory.Create(
                    DEFAULT_ENCRYPTION_VERSION
                );

                byte[] saltBytes = RandomNumberGenerator.GetBytes(32);
                byte[] derivedKey = encryptionService.DeriveKeyFromPassword(masterKey, saltBytes);
                vaultKey = RandomNumberGenerator.GetBytes(32);

                (byte[] encryptedVaultKey, byte[] iv) = encryptionService.EncryptAes(
                    vaultKey,
                    derivedKey
                );

                bool vaultCreated = _shelterVault.CreateShelterVault(
                    uuid,
                    name,
                    encryptedVaultKey.ToBase64(),
                    iv.ToBase64(),
                    saltBytes.ToBase64(),
                    DEFAULT_ENCRYPTION_VERSION,
                    name
                );
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
