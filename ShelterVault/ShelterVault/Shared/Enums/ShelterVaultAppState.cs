using ShelterVault.Shared.Attributes;
using ShelterVault.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
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
