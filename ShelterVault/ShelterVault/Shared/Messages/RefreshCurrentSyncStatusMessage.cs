using CommunityToolkit.Mvvm.Messaging.Messages;
using ShelterVault.Shared.Enums;

namespace ShelterVault.Shared.Messages
{
    public class RefreshCurrentSyncStatusMessage : ValueChangedMessage<CloudSyncStatus>
    {
        public RefreshCurrentSyncStatusMessage(CloudSyncStatus value) : base(value)
        {
        }
    }
}
