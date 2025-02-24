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
    public interface IVaultReaderManager
    {
        IList<VaultModel> GetCurrentVault();
        ShelterVaultModel GetVaultById(string uuid);
    }

    public class VaultReaderManager : IVaultReaderManager
    {
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;

        public VaultReaderManager(IShelterVaultLocalStorage shelterVaultLocalStorage)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
        }

        public IList<VaultModel> GetCurrentVault()
        {
            List<VaultModel> vaults = new List<VaultModel>();
            ShelterVaultModel vault = _shelterVaultLocalStorage.GetCurrentVault();
            IEnumerable<ShelterVaultCredentialsModel> credentials = _shelterVaultLocalStorage.GetAllActiveCredentials(vault.UUID);
            VaultModel vaultModel = new(vault, credentials);
            vaults.Add(vaultModel);

            return vaults;
        }

        public ShelterVaultModel GetVaultById(string uuid)
        {
            ShelterVaultModel shelterVaultModel = _shelterVaultLocalStorage.GetVaultByUUID(uuid);
            return shelterVaultModel;
        }
    }
}
