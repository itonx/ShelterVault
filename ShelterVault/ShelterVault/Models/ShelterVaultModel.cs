using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    internal interface IShelterVaultLocalModel
    {
        ICosmosDBModel ToCosmosDBModel();
    }

    public class ShelterVaultModel : IShelterVaultLocalModel
    {
        public string UUID { get; set; }
        public string Name { get; set; }
        public string MasterKeyHash { get; set; }
        public string Iv { get; set; }
        public string Salt { get; set; }
        public long Version { get; set; }
        public int CloudProvider { get; set; }

        public ICosmosDBModel ToCosmosDBModel()
        {
            CosmosDBVault vault = new(UUID, Name, MasterKeyHash, Iv, Salt, Version);
            return vault;
        }
    }
}
