using CommunityToolkit.Mvvm.Messaging.Messages;
using ShelterVault.Models;
using ShelterVault.Shared.Enums;

namespace ShelterVault.Shared.Messages
{
    public class LoadCredentialsMessage : ValueChangedMessage<LoadPagePayload>
    {
        public LoadCredentialsMessage(LoadPagePayload value)
            : base(value) { }

        public LoadCredentialsMessage(ShelterVaultPage value)
            : base(LoadPagePayload.Create(value)) { }
    }
}
