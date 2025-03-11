using Desktiny.UI.Extensions;
using Desktiny.UI.Services;
using Microsoft.UI.Xaml.Data;
using ShelterVault.Shared.Enums;
using System;
using System.ComponentModel;

namespace ShelterVault.Shared.Converters
{
    public class CloudSyncStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CloudSyncStatus cloudSyncStatus && cloudSyncStatus != CloudSyncStatus.None)
                return LangService.GetLangValue(cloudSyncStatus.GetAttribute<DescriptionAttribute>()?.Description ?? string.Empty);

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
