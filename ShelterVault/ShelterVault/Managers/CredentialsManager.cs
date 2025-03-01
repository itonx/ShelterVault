using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShelterVault.Managers
{
    public interface ICredentialsManager
    {
        Task<Credentials> InsertCredentials(Credentials credentials);
        Task<Credentials> UpdateCredentials(Credentials credentials);
        Credentials GetCredentials(CredentialsViewItem credentialsViewItem);
        Credentials GetCredentials(string uuid, bool active = true);
        Task<bool> DeleteCredentials(string uuid);
        IEnumerable<CredentialsViewItem> GetAllActiveCredentials(string shelterVaultUuid);
    }

    public class CredentialsManager : ICredentialsManager
    {
        private readonly IShelterVaultCredentials _shelterVaultCredentials;
        private readonly IEncryptionService _encryptionService;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly ICloudSyncManager _cloudSyncManager;

        public CredentialsManager(IShelterVaultStateService shelterVaultStateService, IShelterVaultCredentials shelterVaultCredentials, IEncryptionService encryptionService, ICloudSyncManager cloudSyncManager)
        {
            _shelterVaultCredentials = shelterVaultCredentials;
            _encryptionService = encryptionService;
            _shelterVaultStateService = shelterVaultStateService;
            _cloudSyncManager = cloudSyncManager;
        }

        public async Task<Credentials> InsertCredentials(Credentials credentials)
        {
            try
            {
                (byte[] derivedKey, byte[] salt) = _shelterVaultStateService.GetLocalEncryptionValues();
                (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(credentials.GetJsonValues(), derivedKey, salt);

                ShelterVaultCredentialsModel shelterVaultCredentials = new(credentials.ShelterVaultUuid, encryptedValues);
                bool inserted = _shelterVaultCredentials.InsertCredentials(shelterVaultCredentials);

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
                (byte[] derivedKey, byte[] salt) = _shelterVaultStateService.GetLocalEncryptionValues();
                (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(credentials.GetJsonValues(), derivedKey, salt);

                ShelterVaultCredentialsModel shelterVaultCredentials = new(credentials, encryptedValues);
                if (await CanSynchronize(shelterVaultCredentials))
                {
                    bool updated = _shelterVaultCredentials.UpdateCredentials(shelterVaultCredentials);

                    if (!updated) return null;

                    string decryptedValues = _encryptionService.DecryptAes(shelterVaultCredentials, derivedKey, salt);
                    await _cloudSyncManager.UpsertItemAsync(shelterVaultCredentials, validateItem: true);
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
            (byte[] derivedKey, byte[] salt) = _shelterVaultStateService.GetLocalEncryptionValues();
            string jsonValues = _encryptionService.DecryptAes(credentialsViewItem, derivedKey, salt);
            return new(jsonValues, credentialsViewItem);
        }

        public Credentials GetCredentials(string uuid, bool active = true)
        {
            ShelterVaultCredentialsModel shelterVaultCredentialsModel = _shelterVaultCredentials.GetCredentialsByUUID(uuid);
            if (shelterVaultCredentialsModel == null || shelterVaultCredentialsModel.Version == -1) return null;
            (byte[] derivedKey, byte[] salt) = _shelterVaultStateService.GetLocalEncryptionValues();

            string decryptedValues = _encryptionService.DecryptAes(shelterVaultCredentialsModel, derivedKey, salt);
            return new(decryptedValues, shelterVaultCredentialsModel);
        }

        public async Task<bool> DeleteCredentials(string uuid)
        {
            try
            {
                ShelterVaultCredentialsModel tmpCredentials = _shelterVaultCredentials.GetCredentialsByUUID(uuid);
                if (await CanSynchronize(tmpCredentials))
                {
                    tmpCredentials.MarkAsDeleted();
                    _shelterVaultCredentials.UpdateCredentials(tmpCredentials);
                    return await _cloudSyncManager.UpsertItemAsync(tmpCredentials, validateItem: true);
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerable<CredentialsViewItem> GetAllActiveCredentials(string shelterVaultUuid)
        {
            IList<CredentialsViewItem> credentialsList = new List<CredentialsViewItem>();
            IEnumerable<ShelterVaultCredentialsModel> shelterVaultCredentials = _shelterVaultCredentials.GetAllActiveCredentials(shelterVaultUuid);
            (byte[] derivedKey, byte[] salt) = _shelterVaultStateService.GetLocalEncryptionValues();

            foreach (ShelterVaultCredentialsModel item in shelterVaultCredentials)
            {
                string jsonValues = _encryptionService.DecryptAes(item, derivedKey, salt);
                CredentialsViewItem credentials = new(item, Credentials.GetCredentialFrom(jsonValues));
                credentialsList.Add(credentials);
            }

            return credentialsList.Any() ? credentialsList : Enumerable.Empty<CredentialsViewItem>();
        }
    }
}
