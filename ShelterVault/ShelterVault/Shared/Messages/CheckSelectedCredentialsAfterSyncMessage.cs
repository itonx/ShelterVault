using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ShelterVault.Shared.Messages
{
    public class CheckSelectedCredentialsAfterSyncMessage : ValueChangedMessage<bool>
    {
        public CheckSelectedCredentialsAfterSyncMessage(bool value) : base(value)
        {
        }
    }
}
