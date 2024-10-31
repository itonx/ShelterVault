using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Attributes;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShelterVault.Shared.Extensions;

namespace ShelterVault.Shared.Behaviors
{
    public class ShelterVaultThemeBehavior : Behavior<Grid>
    {
        public static readonly DependencyProperty CurrentShelterVaultThemeProperty =
            DependencyProperty.RegisterAttached(
            "CurrentShelterVaultTheme",
            typeof(ShelterVaultTheme),
            typeof(ShelterVaultThemeBehavior),
            new PropertyMetadata(null, OnCurrentShelterVaultThemeChanged));

        private static void OnCurrentShelterVaultThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Grid container = d as Grid;
            ShelterVaultTheme currentShelterVaultTheme = (ShelterVaultTheme)e.NewValue;
            ThemeStyleAttribute currentShelterVaultThemeConfig = currentShelterVaultTheme.GetAttribute<ThemeStyleAttribute>();
            MainWindow mainWindow = WindowHelper.CurrentMainWindow;
            ResourceDictionary lastDictionary = Application.Current.Resources.MergedDictionaries.LastOrDefault();

            ElementTheme expectedTheme = currentShelterVaultThemeConfig.SupportedThemeStyle;
            ElementTheme switchTo = expectedTheme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;

            if (lastDictionary != null && lastDictionary.Source != null && lastDictionary.Source.OriginalString.Contains("ms-appx:///Resources/"))
            {
                Application.Current.Resources.MergedDictionaries.Remove(lastDictionary);
            }

            if (currentShelterVaultTheme == ShelterVaultTheme.LIGHT || currentShelterVaultTheme == ShelterVaultTheme.DARK)
            {
                expectedTheme = currentShelterVaultThemeConfig.SupportedThemeStyle;
                switchTo = expectedTheme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;
            }
            else
            {
                ResourceDictionary resourceTheme = new ResourceDictionary()
                {
                    Source = new Uri($"ms-appx:///Resources/{currentShelterVaultThemeConfig.ThemeName}.xaml")
                };
                Application.Current.Resources.MergedDictionaries.Add(resourceTheme);
            }

            container.Opacity = 0;
            container.RequestedTheme = switchTo;
            container.RequestedTheme = expectedTheme;
            container.AnimateOpacity(0, 1, 1);
        }

        public static void SetCurrentShelterVaultTheme(Grid obj, ShelterVaultTheme value)
        {
            obj.SetValue(CurrentShelterVaultThemeProperty, value);
        }

        public static ShelterVaultTheme GetCurrentShelterVaultTheme(Grid obj)
        {
            return (ShelterVaultTheme)obj.GetValue(CurrentShelterVaultThemeProperty);
        }
    }
}
