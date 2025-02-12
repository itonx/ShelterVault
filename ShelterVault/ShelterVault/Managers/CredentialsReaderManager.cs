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
    internal interface ICredentialsReaderManager
    {
        IEnumerable<CredentialsViewItem> GetAllCredentials(string shelterVaultUuid);
    }

    internal class CredentialsReaderManager : ICredentialsReaderManager
    {
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IEncryptionService _encryptionService;
        private readonly IShelterVaultStateService _shelterVaultStateService;

        public CredentialsReaderManager(IShelterVaultStateService shelterVaultStateService, IShelterVaultLocalStorage shelterVaultLocalStorage, IEncryptionService encryptionService)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _encryptionService = encryptionService;
            _shelterVaultStateService = shelterVaultStateService;
        }

        public IEnumerable<CredentialsViewItem> GetAllCredentials(string shelterVaultUuid)
        {
            IList<CredentialsViewItem> credentialsList = new List<CredentialsViewItem>();
            IEnumerable<ShelterVaultCredentialsModel> shelterVaultCredentials = _shelterVaultLocalStorage.GetAllCredentials(shelterVaultUuid);
            byte[] masterKey = _shelterVaultStateService.GetMasterKeyUnprotected();
            byte[] salt = _shelterVaultStateService.GetMasterKeySaltUnprotected();

            foreach (ShelterVaultCredentialsModel item in shelterVaultCredentials)
            {
                string jsonValues = _encryptionService.DecryptAes(item, masterKey, salt);
                CredentialsViewItem credentials = new(item, Credentials.GetCredentialFrom(jsonValues));
                credentialsList.Add(credentials);
            }

            return credentialsList.Any() ? credentialsList : Enumerable.Empty<CredentialsViewItem>();
        }
    }
}
