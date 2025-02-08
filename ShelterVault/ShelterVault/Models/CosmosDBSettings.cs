using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    internal record CosmosDBSettings
    (
        string CosmosEndpoint,
        string CosmosKey,
        string CosmosDatabase,
        string CosmosContainer
    )
    {
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(CosmosEndpoint) &&
                !string.IsNullOrWhiteSpace(CosmosKey) &&
                !string.IsNullOrWhiteSpace(CosmosDatabase) &&
                !string.IsNullOrWhiteSpace(CosmosContainer);
        }
    }
}
