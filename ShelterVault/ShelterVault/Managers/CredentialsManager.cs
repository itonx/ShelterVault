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
        private readonly IMasterKeyService _masterKeyService;

        public CredentialsManager(IMasterKeyService masterKeyService, IShelterVaultLocalStorage shelterVaultLocalStorage, IEncryptionService encryptionService)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _encryptionService = encryptionService;
            _masterKeyService = masterKeyService;
        }

        public Credentials InsertCredentials(Credentials credentials)
        {
            byte[] masterKey = _masterKeyService.GetMasterKeyUnprotected();
            byte[] salt = _masterKeyService.GetMasterKeySaltUnprotected();

            (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(credentials.GetJsonValues(), masterKey, salt);

            ShelterVaultCredentialsModel shelterVaultCredentials = new(encryptedValues);
            bool inserted = _shelterVaultLocalStorage.InsertCredentials(shelterVaultCredentials);

            if (!inserted) return null;

            credentials.UUID = shelterVaultCredentials.UUID;
            return credentials;
        }

        public Credentials UpdateCredentials(Credentials credentials)
        {
            byte[] masterKey = _masterKeyService.GetMasterKeyUnprotected();
            byte[] salt = _masterKeyService.GetMasterKeySaltUnprotected();

            (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(credentials.GetJsonValues(), masterKey, salt);

            ShelterVaultCredentialsModel shelterVaultCredentials = new(credentials, encryptedValues);
            bool updated = _shelterVaultLocalStorage.UpdateCredentials(shelterVaultCredentials);

            if(!updated) return null;

            string decryptedValues = _encryptionService.DecryptAes(shelterVaultCredentials.EncryptedValues.FromBase64ToBytes(), masterKey, shelterVaultCredentials.Iv.FromBase64ToBytes(), salt);
            return new(decryptedValues, shelterVaultCredentials);
        }

        public Credentials GetCredentials(CredentialsViewItem credentialsViewItem)
        {
            byte[] masterKey = _masterKeyService.GetMasterKeyUnprotected();
            byte[] salt = _masterKeyService.GetMasterKeySaltUnprotected();

            string jsonValues = _encryptionService.DecryptAes(credentialsViewItem.EncryptedValues.FromBase64ToBytes(), masterKey, credentialsViewItem.Iv.FromBase64ToBytes(), salt);
            return new(jsonValues, credentialsViewItem);
        }

        public bool DeleteCredentials(string uuid)
        {
            return _shelterVaultLocalStorage.DeleteCredentials(uuid);
        }
    }
}
