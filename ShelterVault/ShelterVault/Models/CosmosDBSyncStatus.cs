using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    internal class CosmosDBSyncStatus
    {
        public bool IsFirstSyncDone { get; set; }

        public CosmosDBSyncStatus(bool isFirstSyncDone)
        {
            IsFirstSyncDone = isFirstSyncDone;
        }
    }
}
