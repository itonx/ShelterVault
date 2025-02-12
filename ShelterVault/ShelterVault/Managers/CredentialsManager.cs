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
        Credentials InsertCredentials(Credentials credentials);
        Credentials UpdateCredentials(Credentials credentials);
        Credentials GetCredentials(CredentialsViewItem credentialsViewItem);
        bool DeleteCredentials(string uuid);
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

        public Credentials InsertCredentials(Credentials credentials)
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
                _cloudSyncManager.UpsertItemAsync(shelterVaultCredentials);
                return credentials;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Credentials UpdateCredentials(Credentials credentials)
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
                _cloudSyncManager.UpsertItemAsync(shelterVaultCredentials);
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

        public bool DeleteCredentials(string uuid)
        {
            try
            {
                ShelterVaultCredentialsModel tmpCredentials = _shelterVaultLocalStorage.GetCredentialsByUUID(uuid);
                _cloudSyncManager.DeleteItemAsync(tmpCredentials);
                return _shelterVaultLocalStorage.DeleteCredentials(uuid);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
