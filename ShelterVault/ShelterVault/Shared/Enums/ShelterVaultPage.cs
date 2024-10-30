using ShelterVault.Shared.Attributes;
using ShelterVault.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
