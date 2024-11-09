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
    internal interface ICredentialsReaderManager
    {
        IEnumerable<CredentialsViewItem> GetAllCredentials();
    }

    internal class CredentialsReaderManager : ICredentialsReaderManager
    {
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IEncryptionService _encryptionService;
        private readonly IMasterKeyService _masterKeyService;

        public CredentialsReaderManager(IMasterKeyService masterKeyService, IShelterVaultLocalStorage shelterVaultLocalStorage, IEncryptionService encryptionService)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _encryptionService = encryptionService;
            _masterKeyService = masterKeyService;
        }

        public IEnumerable<CredentialsViewItem> GetAllCredentials()
        {
            IList<CredentialsViewItem> credentialsList = new List<CredentialsViewItem>();
            IEnumerable<ShelterVaultCredentialsModel> shelterVaultCredentials = _shelterVaultLocalStorage.GetAllCredentials();
            byte[] masterKey = _masterKeyService.GetMasterKeyUnprotected();
            byte[] salt = _masterKeyService.GetMasterKeySaltUnprotected();

            foreach (ShelterVaultCredentialsModel item in shelterVaultCredentials)
            {
                string jsonValues = _encryptionService.DecryptAes(item.EncryptedValues.FromBase64ToBytes(), masterKey, item.Iv.FromBase64ToBytes(), salt);
                CredentialsViewItem credentials = new(item, Credentials.GetCredentialFrom(jsonValues));
                credentialsList.Add(credentials);
            }

            return credentialsList.Any() ? credentialsList : Enumerable.Empty<CredentialsViewItem>();
        }
    }
}
