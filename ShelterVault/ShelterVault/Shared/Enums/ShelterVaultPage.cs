using Desktiny.WinUI.Attributes;
using ShelterVault.Views;

namespace ShelterVault.Shared.Enums
{
    public enum ShelterVaultPage
    {
        [PageType(typeof(HomePage))]
        HOME,
        [PageType(typeof(SettingsPage))]
        SETTINGS
    }
}
