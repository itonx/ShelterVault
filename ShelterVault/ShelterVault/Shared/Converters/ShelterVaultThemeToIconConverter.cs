﻿using Microsoft.UI.Xaml.Data;
using ShelterVault.Shared.Attributes;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using System;

namespace ShelterVault.Shared.Converters
{
    public class ShelterVaultThemeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ShelterVaultTheme shelterVaultTheme = (ShelterVaultTheme)value;
            return shelterVaultTheme.GetAttribute<ThemeStyleAttribute>().Icon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
