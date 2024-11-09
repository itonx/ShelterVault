﻿using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ThemeStyleAttribute : Attribute
    {
        public ElementTheme SupportedThemeStyle { get; }
        public string ThemeName { get; }
        public string Icon { get; }

        public ThemeStyleAttribute(ElementTheme pageType, string themeName, string icon)
        {
            SupportedThemeStyle = pageType;
            ThemeName = themeName;
            Icon = icon;
        }
    }
}