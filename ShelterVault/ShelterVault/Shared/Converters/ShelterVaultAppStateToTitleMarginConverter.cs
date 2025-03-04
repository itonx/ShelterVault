using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using ShelterVault.Shared.Enums;
using System;

namespace ShelterVault.Shared.Converters
{
    public class ShelterVaultAppStateToTitleMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ShelterVaultAppState appState && appState == ShelterVaultAppState.NavigationView) return new Thickness(0);

            return new Thickness(10, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
