using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Enums
{
    enum CloudSyncStatus
    {
        [Description("Configuration pending")]
        ConfigurationPending,
        [Description("Analyzing")]
        Analyzing,
        [Description("Sync in process")]
        SynchInProcess,
        [Description("Sync faield")]
        SynchFailed,
        [Description("Sync completed")]
        SynchCompleted,
        [Description("Up to date")]
        UpToDate,
        [Description("Outdated")]
        Outdated,
    }
}
