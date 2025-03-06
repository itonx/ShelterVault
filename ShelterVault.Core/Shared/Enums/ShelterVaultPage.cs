using ShelterVault.Core.Shared.Interfaces;
using ShelterVault.Shared.Attributes;

namespace ShelterVault.Shared.Enums
{
    public enum ShelterVaultPage
    {
        [PageType(typeof(IHomePage))]
        HOME,
        [PageType(typeof(ISettingsPage))]
        SETTINGS
    }
}
