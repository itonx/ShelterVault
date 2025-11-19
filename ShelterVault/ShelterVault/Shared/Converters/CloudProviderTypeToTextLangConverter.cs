using Desktiny.WinUI.Extensions;
using Desktiny.WinUI.Services;
using Microsoft.UI.Xaml.Data;
using ShelterVault.Shared.Enums;
using System;
using System.ComponentModel;

namespace ShelterVault.Shared.Converters
{
    internal class CloudProviderTypeToTextLangConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CloudProviderType cloudProviderType)
                return LangService.GetLangValue(cloudProviderType.GetAttribute<DescriptionAttribute>()?.Description ?? string.Empty);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
