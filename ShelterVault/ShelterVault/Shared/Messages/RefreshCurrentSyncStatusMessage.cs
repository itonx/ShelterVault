using CommunityToolkit.Mvvm.Messaging.Messages;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Messages
{
    public class RefreshCurrentSyncStatusMessage : ValueChangedMessage<CloudSyncStatus>
    {
        public RefreshCurrentSyncStatusMessage(CloudSyncStatus value) : base(value)
        {
        }
    }
}
