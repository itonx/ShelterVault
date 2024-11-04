﻿using Microsoft.UI.Xaml.Controls;
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
            DependencyProperty.Register(
                nameof(CurrentShelterVaultTheme),
                typeof(ShelterVaultTheme),
                typeof(ShelterVaultThemeBehavior),
                new PropertyMetadata(null, OnCurrentShelterVaultThemeChanged));

        public ShelterVaultTheme CurrentShelterVaultTheme
        {
            get { return (ShelterVaultTheme)GetValue(CurrentShelterVaultThemeProperty); }
            set { SetValue(CurrentShelterVaultThemeProperty, value); }
        }

        private static void OnCurrentShelterVaultThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ShelterVaultThemeBehavior behavior = d as ShelterVaultThemeBehavior;
            Grid container = behavior.AssociatedObject;
            ShelterVaultTheme currentShelterVaultTheme = (ShelterVaultTheme)e.NewValue;
            ThemeStyleAttribute currentShelterVaultThemeConfig = currentShelterVaultTheme.GetAttribute<ThemeStyleAttribute>();
            ResourceDictionary lastDictionary = Application.Current.Resources.MergedDictionaries.LastOrDefault();

            ElementTheme expectedTheme = currentShelterVaultThemeConfig.SupportedThemeStyle;
            ElementTheme switchTo = expectedTheme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;

            if (lastDictionary != null && lastDictionary.Source != null && lastDictionary.Source.OriginalString.Contains("ms-appx:///Resources/") && !lastDictionary.Source.OriginalString.Contains("OverrideDefaultTheme.xaml"))
            {
                Application.Current.Resources.MergedDictionaries.Remove(lastDictionary);
            }

            if (currentShelterVaultTheme != ShelterVaultTheme.LIGHT && currentShelterVaultTheme != ShelterVaultTheme.DARK)
            {
                ResourceDictionary resourceTheme = new ResourceDictionary()
                {
                    Source = new Uri($"ms-appx:///Resources/{currentShelterVaultThemeConfig.ThemeName}.xaml")
                };
                Application.Current.Resources.MergedDictionaries.Add(resourceTheme);
            }

            container.RequestedTheme = switchTo;
            container.RequestedTheme = expectedTheme;
        }
    }
}
