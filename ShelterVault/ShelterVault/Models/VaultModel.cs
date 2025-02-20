using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    public class VaultModel
    {
        public ShelterVaultModel ShelterVault { get; set; }
        public IEnumerable<ShelterVaultCredentialsModel> ShelterVaultCredentials { get; set; }

        public VaultModel(ShelterVaultModel shelterVault, IEnumerable<ShelterVaultCredentialsModel> shelterVaultCredentials)
        {
            ShelterVault = shelterVault;
            ShelterVaultCredentials = shelterVaultCredentials;
        }
    }
}
