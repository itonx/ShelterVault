using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;
using System.ComponentModel;

namespace ShelterVault.Shared.Converters
{
    internal class CloudSyncStatusToTextConverter : IValueConverter
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
