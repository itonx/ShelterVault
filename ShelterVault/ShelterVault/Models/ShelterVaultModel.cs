using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    internal class ShelterVaultModel
    {
        public string UUID { get; set; }
        public string Name { get; set; }
        public string MasterKeyHash { get; set; }
        public string Iv { get; set; }
        public string Salt { get; set; }

        public CosmosDBVault ToCosmosDBVault()
        {
            CosmosDBVault vault = new(UUID, Name, MasterKeyHash, Iv, Salt);
            return vault;
        }
    }
}
