using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using ShelterVault.Shared.Enums;
using System;

namespace ShelterVault.Shared.Converters
{
    public class CloudProviderTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CloudProviderType cloudProviderType && parameter != null && int.TryParse(parameter.ToString(), out int selectedCloudProviderType) && cloudProviderType == (CloudProviderType)selectedCloudProviderType)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
