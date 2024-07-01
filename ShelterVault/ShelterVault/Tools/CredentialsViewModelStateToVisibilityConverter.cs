﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using ShelterVault.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Tools
{
    public class CredentialsViewModelStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is CredentialsViewModelState state && parameter != null)
            {
                foreach (var item in parameter.ToString().Split(":"))
                    if (state == (CredentialsViewModelState)Enum.Parse(typeof(CredentialsViewModelState), item.ToString())) return true;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
