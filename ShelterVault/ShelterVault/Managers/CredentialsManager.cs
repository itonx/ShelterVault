using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Managers
{
    internal interface ICredentialsManager
    {
        Task<Credentials> InsertCredentials(Credentials credentials);
        Task<Credentials> UpdateCredentials(Credentials credentials);
        Credentials GetCredentials(CredentialsViewItem credentialsViewItem);
        Credentials GetCredentials(string uuid, bool active = true);
        Task<bool> DeleteCredentials(string uuid);
    }

    internal class CredentialsManager : ICredentialsManager
    {
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IEncryptionService _encryptionService;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly ICloudSyncManager _cloudSyncManager;

        public CredentialsManager(IShelterVaultStateService shelterVaultStateService, IShelterVaultLocalStorage shelterVaultLocalStorage, IEncryptionService encryptionService, ICloudSyncManager cloudSyncManager)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _encryptionService = encryptionService;
            _shelterVaultStateService = shelterVaultStateService;
            _cloudSyncManager = cloudSyncManager;
        }

        public async Task<Credentials> InsertCredentials(Credentials credentials)
        {
            try
            {
                byte[] masterKey = _shelterVaultStateService.GetMasterKeyUnprotected();
                byte[] salt = _shelterVaultStateService.GetMasterKeySaltUnprotected();

                (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(credentials.GetJsonValues(), masterKey, salt);

                ShelterVaultCredentialsModel shelterVaultCredentials = new(credentials.ShelterVaultUuid, encryptedValues);
                bool inserted = _shelterVaultLocalStorage.InsertCredentials(shelterVaultCredentials);

                if (!inserted) return null;

                credentials.UUID = shelterVaultCredentials.UUID;
                credentials.Iv = shelterVaultCredentials.Iv;
                await _cloudSyncManager.UpsertItemAsync(shelterVaultCredentials);
                return credentials;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Credentials> UpdateCredentials(Credentials credentials)
        {
            try
            {
                byte[] masterKey = _shelterVaultStateService.GetMasterKeyUnprotected();
                byte[] salt = _shelterVaultStateService.GetMasterKeySaltUnprotected();

                (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(credentials.GetJsonValues(), masterKey, salt);

                ShelterVaultCredentialsModel shelterVaultCredentials = new(credentials, encryptedValues);
                if(await CanSynchronize(shelterVaultCredentials))
                {
                    bool updated = _shelterVaultLocalStorage.UpdateCredentials(shelterVaultCredentials);

                    if(!updated) return null;

                    string decryptedValues = _encryptionService.DecryptAes(shelterVaultCredentials, masterKey, salt);
                    await _cloudSyncManager.UpsertItemAsync(shelterVaultCredentials);
                    return new(decryptedValues, shelterVaultCredentials);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<bool> CanSynchronize(ShelterVaultCredentialsModel shelterVaultCredentials)
        {
            ICosmosDBModel cosmosDBModel = await _cloudSyncManager.GetItemAsync(shelterVaultCredentials);
            return cosmosDBModel == null || cosmosDBModel.version <= shelterVaultCredentials.Version;
        }

        public Credentials GetCredentials(CredentialsViewItem credentialsViewItem)
        {
            byte[] masterKey = _shelterVaultStateService.GetMasterKeyUnprotected();
            byte[] salt = _shelterVaultStateService.GetMasterKeySaltUnprotected();

            string jsonValues = _encryptionService.DecryptAes(credentialsViewItem, masterKey, salt);
            return new(jsonValues, credentialsViewItem);
        }

        public Credentials GetCredentials(string uuid, bool active = true)
        {
            ShelterVaultCredentialsModel shelterVaultCredentialsModel = _shelterVaultLocalStorage.GetCredentialsByUUID(uuid);
            if(shelterVaultCredentialsModel == null || shelterVaultCredentialsModel.Version == -1) return null;
            byte[] masterKey = _shelterVaultStateService.GetMasterKeyUnprotected();
            byte[] salt = _shelterVaultStateService.GetMasterKeySaltUnprotected();

            string decryptedValues = _encryptionService.DecryptAes(shelterVaultCredentialsModel, masterKey, salt);
            return new(decryptedValues, shelterVaultCredentialsModel);
        }

        public async Task<bool> DeleteCredentials(string uuid)
        {
            try
            {
                ShelterVaultCredentialsModel tmpCredentials = _shelterVaultLocalStorage.GetCredentialsByUUID(uuid);
                if(await CanSynchronize(tmpCredentials))
                {
                    tmpCredentials.MarkAsDeleted();
                    _shelterVaultLocalStorage.UpdateCredentials(tmpCredentials);
                    return await _cloudSyncManager.UpsertItemAsync(tmpCredentials);
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
