using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ShelterVault.Shared.Messages
{
    public class ShowSyncStatusMessage : ValueChangedMessage<bool>
    {
        public ShowSyncStatusMessage(bool value) : base(value)
        {
        }
    }
}
