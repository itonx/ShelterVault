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
            Enum.TryParse(typeof(ShelterVaultTheme), theme, true, out object? shelterVaultThemeObj);
            ShelterVaultTheme? shelterVaultTheme = (ShelterVaultTheme?)shelterVaultThemeObj;

            if (shelterVaultTheme == null && PInvoke.ShouldSystemUseDarkMode()) return ShelterVaultTheme.DARK;

            return (ShelterVaultTheme)shelterVaultTheme;
        }

        public ShelterVaultTheme GetNextTheme(ShelterVaultTheme currentShelterVaultTheme)
        {
            ShelterVaultTheme[] shelterVaultThemeValues = (ShelterVaultTheme[])Enum.GetValues(typeof(ShelterVaultTheme));
            int currentIndex = Array.IndexOf(shelterVaultThemeValues, currentShelterVaultTheme);
            ShelterVaultTheme nextShelterVaultTheme = shelterVaultThemeValues[0];

            if (currentIndex + 1 < shelterVaultThemeValues.Length)
            {
                nextShelterVaultTheme = shelterVaultThemeValues[currentIndex + 1];
            }

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[ShelterVaultConstants.SETTINGS_THEME_KEY] = nextShelterVaultTheme.ToString();
            return GetTheme();
        }
    }
}
