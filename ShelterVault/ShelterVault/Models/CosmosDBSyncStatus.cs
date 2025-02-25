using ShelterVault.Shared.Enums;

namespace ShelterVault.Models
{
    public class CosmosDBSyncStatus
    {
        public CloudSyncStatus CurrentSyncStatus { get; set; }
        public bool IsSyncEnabled { get; set; }

        public CosmosDBSyncStatus()
        {
            CurrentSyncStatus = CloudSyncStatus.None;
            IsSyncEnabled = false;
        }

        public CosmosDBSyncStatus(ShelterVaultSyncStatusModel shelterVaultSyncStatusModel)
        {
            CurrentSyncStatus = shelterVaultSyncStatusModel?.SyncStatus ?? CloudSyncStatus.None;
            IsSyncEnabled = shelterVaultSyncStatusModel?.IsSyncEnabled ?? false;
        }
    }
}
