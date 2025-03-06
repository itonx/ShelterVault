using CommunityToolkit.Mvvm.Messaging.Messages;
using ShelterVault.Shared.Enums;

namespace ShelterVault.Shared.Messages
{
    public class ShowPageRequestMessage : ValueChangedMessage<ShelterVaultPage>
    {
        public ShowPageRequestMessage(ShelterVaultPage value) : base(value)
        {
        }
    }
}
