using Microsoft.UI.Xaml;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
 
namespace ShelterVault.Services
{
    public interface IShelterVaultThemeService
    {
        ShelterVaultTheme GetTheme();
        ShelterVaultTheme GetNextTheme(ShelterVaultTheme currentShelterVaultTheme);
    }

    public class ShelterVaultThemeService : IShelterVaultThemeService
    {
        public ShelterVaultTheme GetTheme()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string theme = localSettings.Values[ShelterVaultConstants.SETTINGS_THEME_KEY] as string;
            ElementTheme elementTheme = theme == null ? ElementTheme.Default : (ElementTheme)Enum.Parse(typeof(ElementTheme), theme);

            if (elementTheme == ElementTheme.Default && PInvoke.ShouldSystemUseDarkMode()) return ShelterVaultTheme.DARK;

            return elementTheme == ElementTheme.Light ? ShelterVaultTheme.LIGHT : ShelterVaultTheme.DARK;
        }

        public ShelterVaultTheme GetNextTheme(ShelterVaultTheme currentShelterVaultTheme)
        {
            ElementTheme newTheme = currentShelterVaultTheme == ShelterVaultTheme.LIGHT ? ElementTheme.Dark : ElementTheme.Light;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[ShelterVaultConstants.SETTINGS_THEME_KEY] = newTheme.ToString();
            return GetTheme();
        }
    }
}
