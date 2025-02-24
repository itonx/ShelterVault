using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    public class CosmosDBSyncStatus
    {
        public CloudSyncStatus CurrentSyncStatus { get; set; } = CloudSyncStatus.None;
        public bool IsSyncEnabled { get; set; } = false;

        public CosmosDBSyncStatus()
        {
            
        }

        public CosmosDBSyncStatus(CloudSyncStatus currentSyncStatus)
        {
            CurrentSyncStatus = currentSyncStatus;
        }

        public CosmosDBSyncStatus(ShelterVaultSyncStatusModel shelterVaultSyncStatusModel)
        {
            CurrentSyncStatus = shelterVaultSyncStatusModel?.SyncStatus ?? CloudSyncStatus.None;
            IsSyncEnabled = shelterVaultSyncStatusModel?.IsSyncEnabled ?? false;
        }
    }
}
