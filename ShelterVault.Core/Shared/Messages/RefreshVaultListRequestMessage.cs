using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ShelterVault.Shared.Messages
{
    public class RefreshVaultListRequestMessage : ValueChangedMessage<bool>
    {
        public RefreshVaultListRequestMessage(bool value) : base(value)
        {
        }
    }
}
