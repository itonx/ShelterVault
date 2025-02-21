using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;
using ShelterVault.ViewModels;
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
                bool updated = _shelterVaultLocalStorage.UpdateCredentials(shelterVaultCredentials);

                if(!updated) return null;

                string decryptedValues = _encryptionService.DecryptAes(shelterVaultCredentials, masterKey, salt);
                await _cloudSyncManager.UpsertItemAsync(shelterVaultCredentials);
                return new(decryptedValues, shelterVaultCredentials);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Credentials GetCredentials(CredentialsViewItem credentialsViewItem)
        {
            byte[] masterKey = _shelterVaultStateService.GetMasterKeyUnprotected();
            byte[] salt = _shelterVaultStateService.GetMasterKeySaltUnprotected();

            string jsonValues = _encryptionService.DecryptAes(credentialsViewItem, masterKey, salt);
            return new(jsonValues, credentialsViewItem);
        }

        public async Task<bool> DeleteCredentials(string uuid)
        {
            try
            {
                ShelterVaultCredentialsModel tmpCredentials = _shelterVaultLocalStorage.GetCredentialsByUUID(uuid);
                tmpCredentials.MarkAsDeleted();
                _shelterVaultLocalStorage.UpdateCredentials(tmpCredentials);
                return await _cloudSyncManager.UpsertItemAsync(tmpCredentials);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
