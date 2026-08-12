using System.Collections.Generic;
using System.Threading.Tasks;
using ShelterVault.Models;

namespace ShelterVault.Interfaces
{
    public interface ICredentialsManager
    {
        Task<Credentials> InsertCredentials(Credentials credentials);
        Task<Credentials> InsertCredentials(
            Credentials credentials,
            byte[] encryptionKey,
            string tmpDb = null,
            int? version = null
        );
        Task<Credentials> UpdateCredentials(Credentials credentials);
        Credentials GetCredentials(CredentialsViewItem credentialsViewItem);
        Credentials GetCredentials(string uuid, bool active = true);
        Task<bool> DeleteCredentials(string uuid);
        IEnumerable<CredentialsViewItem> GetAllActiveCredentials(string shelterVaultUuid);
    }
}
