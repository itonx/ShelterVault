using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    internal interface ICosmosDBModel
    {
        public string id { get; set; }
        public string type { get; set; }
    }
    internal record CosmosDBVault
    (
        string id,
        string name,
        string masterKeyHash,
        string iv,
        string salt
    ) : ICosmosDBModel
    {
        public string id { get; set; } = id;
        public string type { get; set; } = "shelter_vault";
    }

    internal record CosmosDBCredentials
    (
        string id,
        string encryptedValues,
        string iv,
        string shelterVaultUuid
    ) : ICosmosDBModel
    {
        public string id { get; set; } = id;
        public string type { get; set; } = "shelter_vault_credentials";
    }
}
