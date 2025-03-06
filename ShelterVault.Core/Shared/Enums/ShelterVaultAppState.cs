using ShelterVault.Core.Shared.Interfaces;
using ShelterVault.Shared.Attributes;

namespace ShelterVault.Shared.Enums
{
    public enum ShelterVaultAppState
    {
        [PageType(typeof(ICreateMasterKeyPage))]
        CreateMasterKey,
        [PageType(typeof(IConfirmMasterKeyPage))]
        ConfirmMasterKey,
        [PageType(typeof(INavigationViewPage))]
        NavigationView
    }
}
