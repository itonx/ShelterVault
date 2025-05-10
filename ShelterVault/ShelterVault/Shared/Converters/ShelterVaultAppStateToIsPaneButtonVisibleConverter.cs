using Microsoft.UI.Xaml.Data;
using ShelterVault.Shared.Enums;
using System;

namespace ShelterVault.Shared.Converters
{
    class ShelterVaultAppStateToIsPaneButtonVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is ShelterVaultAppState appState && appState == ShelterVaultAppState.NavigationView;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
