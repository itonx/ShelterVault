using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public CloudSyncInformation(CosmosDBSyncStatus cosmosDBSyncStatus)
        {
            CurrentSyncStatus = cosmosDBSyncStatus.CurrentSyncStatus;
            HasCloudConfiguration = cosmosDBSyncStatus.CurrentSyncStatus != CloudSyncStatus.None;
        }

        public string GetSyncStatusLangKey()
        {
            return CurrentSyncStatus.GetAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
        }
    }
}
