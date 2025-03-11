using Microsoft.UI.Xaml;
using System;

namespace Desktiny.UI.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ThemeStyleAttribute : Attribute
    {
        public ElementTheme AppTheme { get; }
        public string ThemeUri { get; }
        public string Icon { get; }

        public ThemeStyleAttribute(ElementTheme appTheme, string themeUri, string icon)
        {
            AppTheme = appTheme;
            ThemeUri = themeUri;
            Icon = icon;
        }
    }
}
