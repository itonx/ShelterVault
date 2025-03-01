using ShelterVault.Shared.Enums;

namespace ShelterVault.Models
{
    public class CloudSyncInformation
    {
        public CloudSyncStatus CurrentSyncStatus { get; set; }
        public bool HasCloudConfiguration { get; set; }
        public bool IsDialogOpen { get; set; }
        public bool CanSynchronize => !IsDialogOpen && HasCloudConfiguration;

        public CloudSyncInformation()
        {
            CurrentSyncStatus = CloudSyncStatus.None;
            HasCloudConfiguration = false;
        }

        public CloudSyncInformation(ShelterVaultSyncStatusModel shelterVaultSyncStatusModel, bool isDialogOpen)
        {
            CurrentSyncStatus = shelterVaultSyncStatusModel.SyncStatus;
            HasCloudConfiguration = shelterVaultSyncStatusModel.IsSyncEnabled;
            IsDialogOpen = isDialogOpen;
        }
    }
}
