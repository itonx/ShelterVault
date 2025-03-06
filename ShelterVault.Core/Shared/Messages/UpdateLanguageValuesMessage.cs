using CommunityToolkit.Mvvm.Messaging.Messages;
using ShelterVault.Shared.Enums;

namespace ShelterVault.Shared.Messages
{
    public class UpdateLanguageValuesMessage : ValueChangedMessage<ShelterVaultLang>
    {
        public UpdateLanguageValuesMessage(ShelterVaultLang value) : base(value)
        {
        }
    }
}
