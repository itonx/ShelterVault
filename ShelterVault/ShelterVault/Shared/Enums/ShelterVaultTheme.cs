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
        [ThemeStyle(Microsoft.UI.Xaml.ElementTheme.Light, "Default")]
        LIGHT,
        [ThemeStyle(Microsoft.UI.Xaml.ElementTheme.Dark, "Default")]
        DARK,
        [ThemeStyle(Microsoft.UI.Xaml.ElementTheme.Dark, "NeuromancerTheme")]
        NEUROMANCER,
    }
}
