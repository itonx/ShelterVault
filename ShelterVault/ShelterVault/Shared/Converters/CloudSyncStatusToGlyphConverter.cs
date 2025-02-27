using Microsoft.UI.Xaml.Data;
using ShelterVault.Shared.Attributes;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using System;

namespace ShelterVault.Shared.Converters
{
    public class CloudSyncStatusToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is CloudSyncStatus cloudSyncStatus && cloudSyncStatus != CloudSyncStatus.None)
                return cloudSyncStatus.GetAttribute<GlyphAttribute>()?.Icon;

            return "\uE783";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
