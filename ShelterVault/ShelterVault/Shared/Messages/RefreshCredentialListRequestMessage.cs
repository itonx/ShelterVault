using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ShelterVault.Shared.Messages
{
    public class RefreshCredentialListRequestMessage : ValueChangedMessage<bool>
    {
        public RefreshCredentialListRequestMessage(bool value) : base(value)
        {
        }
    }
}
