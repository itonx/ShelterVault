using ShelterVault.Core.Shared.Enums;
using ShelterVault.Shared.Attributes;

namespace ShelterVault.Shared.Enums
{
    public enum ShelterVaultTheme
    {
        [ThemeStyle(ElementTheme.Light, "OverrideDefaultTheme", "\uE793")]
        LIGHT,
        [ThemeStyle(ElementTheme.Dark, "OverrideDefaultTheme", "\uF0CE")]
        DARK,
        [ThemeStyle(ElementTheme.Dark, "NeuromancerTheme", "\uE950")]
        NEUROMANCER,
    }
}
