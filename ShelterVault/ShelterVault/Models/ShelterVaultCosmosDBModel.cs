using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    internal record CosmosDBVault
    (
        string id,
        string name,
        string masterKeyHash,
        string iv,
        string salt,
        string type = "shelter_vault"
    );

    internal record CosmosDBCredentials
    (
        string id,
        string encryptedValues,
        string iv,
        string shelterVaultUuid,
        string type = "shelter_vault_credentials"
    );
}
