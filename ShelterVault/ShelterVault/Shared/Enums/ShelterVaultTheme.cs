using ShelterVault.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.Shared.Enums
{
    public enum ShelterVaultTheme
    {
        [ThemeStyle(Microsoft.UI.Xaml.ElementTheme.Light, "OverrideDefaultTheme", "\uE793")]
        LIGHT,
        [ThemeStyle(Microsoft.UI.Xaml.ElementTheme.Dark, "OverrideDefaultTheme", "\uF0CE")]
        DARK,
        [ThemeStyle(Microsoft.UI.Xaml.ElementTheme.Dark, "NeuromancerTheme", "\uE950")]
        NEUROMANCER,
    }
}
