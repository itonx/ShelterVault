using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ShelterVault.Shared.Messages
{
    public class CloudProviderChangedMessage : ValueChangedMessage<bool>
    {
        public CloudProviderChangedMessage(bool value) : base(value)
        {
        }
    }
}
