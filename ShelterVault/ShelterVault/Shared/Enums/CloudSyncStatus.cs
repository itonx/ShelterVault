using ShelterVault.Shared.Attributes;
using ShelterVault.Shared.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Enums
{
    public enum CloudSyncStatus
    {
        [Description("")]
        [GlyphAttribute("\uE783")]
        None,

        [Description(LangResourceKeys.SYNC_PENDING)]
        [GlyphAttribute("\uE814")]
        PendingConfiguration,

        [Description(LangResourceKeys.SYNC_IN_PROGRESS)]
        [GlyphAttribute("\uEDAB")]
        SynchInProcess,

        [Description(LangResourceKeys.SYNC_FAILED)]
        [GlyphAttribute("\uEA6A")]
        SynchFailed,

        [Description(LangResourceKeys.SYNC_UP_TO_DATE)]
        [GlyphAttribute("\uE753")]
        UpToDate
    }
}
