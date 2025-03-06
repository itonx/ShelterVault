using CommunityToolkit.Mvvm.Messaging.Messages;
using ShelterVault.Shared.Enums;

namespace ShelterVault.Shared.Messages
{
    public class CurrentAppStateRequestMessage : ValueChangedMessage<ShelterVaultAppState>
    {
        public CurrentAppStateRequestMessage(ShelterVaultAppState value) : base(value)
        {
        }
    }
}
