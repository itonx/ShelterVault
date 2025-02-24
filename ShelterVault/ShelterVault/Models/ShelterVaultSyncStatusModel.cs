using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public ShelterVaultSyncStatusModel(string name, long timestamp, bool isSyncEnabled, CloudSyncStatus syncStatus)
        {
            Name = name;
            Timestamp = timestamp;
            SyncStatus = syncStatus;
            IsSyncEnabled = isSyncEnabled;
        }
    }
}
