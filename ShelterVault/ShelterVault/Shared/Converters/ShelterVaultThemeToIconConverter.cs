using Desktiny.UI.Attributes;
using Desktiny.UI.Extensions;
using Desktiny.UI.Models;
using Microsoft.UI.Xaml.Data;
using ShelterVault.Shared.Extensions;
using System;

namespace ShelterVault.Shared.Converters
{
    public class ShelterVaultThemeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            AppThemeModel currentAppTheme = (AppThemeModel)value;
            return currentAppTheme.GetShelterVaultThemeEquivalent().GetAttribute<ThemeStyleAttribute>().Icon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
