using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace ShelterVault.Shared.Converters
{
    public class ShowHideToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool val && val == bool.Parse(parameter?.ToString() ?? "false") ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}