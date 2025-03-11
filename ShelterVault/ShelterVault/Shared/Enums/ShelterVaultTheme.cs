using Desktiny.UI.Attributes;

namespace ShelterVault.Shared.Enums
{
    public enum ShelterVaultTheme
    {
        [ThemeStyle(Microsoft.UI.Xaml.ElementTheme.Light, "ms-appx:///Resources/OverrideWinUITheme.xaml", "\uE793")]
        LIGHT,
        [ThemeStyle(Microsoft.UI.Xaml.ElementTheme.Dark, "ms-appx:///Resources/OverrideWinUITheme.xaml", "\uF0CE")]
        DARK,
        [ThemeStyle(Microsoft.UI.Xaml.ElementTheme.Dark, "ms-appx:///Resources/NeuromancerTheme.xaml", "\uE950")]
        NEUROMANCER,
    }
}
