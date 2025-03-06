using ShelterVault.Shared.Enums;

namespace ShelterVault.Models
{
    public class ShelterVaultSyncStatusModel
    {
        public string Name { get; set; }
        public long Timestamp { get; set; }
        public bool IsSyncEnabled { get; set; }
        public CloudSyncStatus SyncStatus { get; set; }

        public ShelterVaultSyncStatusModel()
        {
            Name = string.Empty;
            Timestamp = 0;
            IsSyncEnabled = false;
            SyncStatus = CloudSyncStatus.None;
        }
    }
}