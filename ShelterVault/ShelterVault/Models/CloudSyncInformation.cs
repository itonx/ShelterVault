using ShelterVault.Shared.Enums;

namespace ShelterVault.Models
{
    public class CloudSyncInformation
    {
        public CloudSyncStatus CurrentSyncStatus { get; set; }
        public bool HasCloudConfiguration { get; set; }

        public CloudSyncInformation()
        {
            CurrentSyncStatus = CloudSyncStatus.None;
            HasCloudConfiguration = false;
        }

        public CloudSyncInformation(ShelterVaultSyncStatusModel shelterVaultSyncStatusModel)
        {
            CurrentSyncStatus = shelterVaultSyncStatusModel.SyncStatus;
            HasCloudConfiguration = shelterVaultSyncStatusModel.IsSyncEnabled;
        }
    }
}
