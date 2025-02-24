using CommunityToolkit.Mvvm.ComponentModel;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    public record CosmosDBSettings
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

    public record SyncStatus
    (
        long Timestamp = 0,
        bool IsSyncEnabled = false,
        CloudSyncStatus Status = CloudSyncStatus.None
    )
    {
        public long Timestamp { get; set; } = Timestamp;
        public bool IsSyncEnabled { get; set; } = IsSyncEnabled;
        public CloudSyncStatus Status { get; set; } = Status;
    }
}
