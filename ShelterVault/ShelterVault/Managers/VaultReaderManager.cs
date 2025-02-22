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
        IList<VaultModel> GetAllActiveVaults();
        IList<VaultModel> GetAllVaults();
        ShelterVaultModel GetVaultById(string uuid);
    }

    public class VaultReaderManager : IVaultReaderManager
    {
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;

        public VaultReaderManager(IShelterVaultLocalStorage shelterVaultLocalStorage)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
        }

        public IList<VaultModel> GetAllActiveVaults()
        {
            List<VaultModel> vaults = new List<VaultModel>();
            foreach (ShelterVaultModel vault in _shelterVaultLocalStorage.GetAllActiveVaults())
            {
                IEnumerable<ShelterVaultCredentialsModel> credentials = _shelterVaultLocalStorage.GetAllActiveCredentials(vault.UUID);
                VaultModel vaultModel = new(vault, credentials);
                vaults.Add(vaultModel);
            }

            return vaults;
        }

        public IList<VaultModel> GetAllVaults()
        {
            List<VaultModel> vaults = new List<VaultModel>();
            foreach (ShelterVaultModel vault in _shelterVaultLocalStorage.GetAllVaults())
            {
                IEnumerable<ShelterVaultCredentialsModel> credentials = _shelterVaultLocalStorage.GetAllCredentials(vault.UUID);
                VaultModel vaultModel = new(vault, credentials);
                vaults.Add(vaultModel);
            }

            return vaults;
        }

        public ShelterVaultModel GetVaultById(string uuid)
        {
            ShelterVaultModel shelterVaultModel = _shelterVaultLocalStorage.GetVaultByUUID(uuid);
            return shelterVaultModel;
        }
    }
}
