using CommunityToolkit.Mvvm.Messaging.Messages;
using ShelterVault.Models;

namespace ShelterVault.Shared.Messages
{
    public class SelectCredentialRequestMessage : ValueChangedMessage<Credentials>
    {
        public SelectCredentialRequestMessage(Credentials value) : base(value)
        {
        }
    }
}
