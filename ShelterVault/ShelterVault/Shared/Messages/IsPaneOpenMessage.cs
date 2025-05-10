using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ShelterVault.Shared.Messages
{
    public class IsPaneOpenMessage : ValueChangedMessage<bool>
    {
        public IsPaneOpenMessage(bool value) : base(value)
        {
        }
    }
}
