using Microsoft.UI.Xaml;
using System;

namespace Desktiny.UI.Models
{
    public class AppThemeModel
    {
        public ElementTheme AppTheme { get; set; }
        public Uri ThemeResource { get; set; }

        public AppThemeModel(ElementTheme appTheme)
        {
            AppTheme = appTheme;
            ThemeResource = null;
        }

        public AppThemeModel(ElementTheme appTheme, string themeResource)
        {
            AppTheme = appTheme;
            ThemeResource = string.IsNullOrWhiteSpace(themeResource) ? null : new Uri(themeResource);
        }
    }
}
