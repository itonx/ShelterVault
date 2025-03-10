using Desktiny.UI.Attributes;
using ShelterVault.Views;

namespace ShelterVault.Shared.Enums
{
    public enum ShelterVaultAppState
    {
        [PageType(typeof(CreateMasterKeyPage))]
        CreateMasterKey,
        [PageType(typeof(ConfirmMasterKeyPage))]
        ConfirmMasterKey,
        [PageType(typeof(NavigationViewPage))]
        NavigationView
    }
}
