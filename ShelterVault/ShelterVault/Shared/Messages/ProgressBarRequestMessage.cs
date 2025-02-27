using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ShelterVault.Shared.Messages
{
    public class ProgressBarRequestMessage : ValueChangedMessage<bool>
    {
        public ProgressBarRequestMessage(bool value) : base(value)
        {
        }
    }
}
