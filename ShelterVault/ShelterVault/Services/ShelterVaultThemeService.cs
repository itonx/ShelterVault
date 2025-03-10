using Desktiny.UI.Attributes;
using Desktiny.UI.Extensions;
using Desktiny.UI.Models;
using Microsoft.UI.Xaml;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using System;
using System.Linq;
using Windows.Storage;

namespace ShelterVault.Services
{
    public interface IShelterVaultThemeService
    {
        AppThemeModel GetTheme();
        AppThemeModel GetNextTheme(AppThemeModel currentAppTheme);
    }

    public class ShelterVaultThemeService : IShelterVaultThemeService
    {
        public AppThemeModel GetTheme()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string theme = localSettings.Values[ShelterVaultConstants.SETTINGS_THEME_KEY] as string;
            Enum.TryParse(typeof(ShelterVaultTheme), theme, true, out object shelterVaultThemeObj);
            ShelterVaultTheme? shelterVaultTheme = (ShelterVaultTheme?)shelterVaultThemeObj;

            if (shelterVaultTheme == null) return new(ElementTheme.Default);

            ThemeStyleAttribute nextShelterVaultThemeAttribute = shelterVaultTheme.GetAttribute<ThemeStyleAttribute>();
            AppThemeModel appThemeModel = new(nextShelterVaultThemeAttribute.AppTheme, nextShelterVaultThemeAttribute.ThemeUri);
            return appThemeModel;
        }

        public AppThemeModel GetNextTheme(AppThemeModel currentAppTheme)
        {
            ShelterVaultTheme[] shelterVaultThemeValues = (ShelterVaultTheme[])Enum.GetValues(typeof(ShelterVaultTheme));
            ShelterVaultTheme currentShelterVaultTheme = currentAppTheme.GetShelterVaultThemeEquivalent();
            ShelterVaultTheme nextShelterVaultTheme = currentShelterVaultTheme;

            for (int i = 0; i < shelterVaultThemeValues.Length; i++)
            {
                if (currentShelterVaultTheme == shelterVaultThemeValues[i])
                {
                    nextShelterVaultTheme = i == shelterVaultThemeValues.Count() - 1 ? shelterVaultThemeValues[0] : shelterVaultThemeValues[i + 1];
                    break;
                }
            }

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[ShelterVaultConstants.SETTINGS_THEME_KEY] = nextShelterVaultTheme.ToString();
            return GetTheme();
        }
    }
}
