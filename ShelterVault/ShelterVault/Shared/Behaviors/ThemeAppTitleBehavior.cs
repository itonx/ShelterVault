using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShelterVault.Shared.Helpers;
using Microsoft.UI.Windowing;
using Windows.UI;

namespace ShelterVault.Shared.Behaviors
{
    public class ThemeAppTitleBehavior : Behavior<Grid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ActualThemeChanged += AssociatedObject_ActualThemeChanged;
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.ActualThemeChanged -= AssociatedObject_ActualThemeChanged;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_ActualThemeChanged(Microsoft.UI.Xaml.FrameworkElement sender, object args)
        {
            ApplyThemeToCaptionButtons(sender.ActualTheme);
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyThemeToCaptionButtons((sender as FrameworkElement).ActualTheme);
        }

        private void ApplyThemeToCaptionButtons(ElementTheme currentTheme)
        {
            AppWindow appWindow = WindowHelper.CurrentAppWindow;
            appWindow.TitleBar.ButtonHoverBackgroundColor = currentTheme == ElementTheme.Light ? Color.FromArgb(50, 0, 0, 0) : Color.FromArgb(50, 255, 255, 255);
            appWindow.TitleBar.ButtonHoverForegroundColor = currentTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            appWindow.TitleBar.ButtonForegroundColor = currentTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        }
    }
}
