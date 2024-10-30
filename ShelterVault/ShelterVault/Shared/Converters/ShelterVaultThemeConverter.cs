using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShelterVault.Shared.Enums;
 
namespace ShelterVault.Shared.Converters
{
    public class ShelterVaultThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is ShelterVaultTheme val && val == ShelterVaultTheme.LIGHT ? ElementTheme.Light: ElementTheme.Dark;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is ElementTheme val && val == ElementTheme.Light? ShelterVaultTheme.LIGHT : ShelterVaultTheme.DARK;
        }
    }
}
