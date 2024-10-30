using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Behaviors
{
    public class AutoAdjustAppTitleBarCaptionsBehavior : Behavior<Grid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            Grid appTitleBar = (sender as Grid);
            appTitleBar.Loaded -= AssociatedObject_Loaded;

            double scaleAdjustment = appTitleBar.XamlRoot.RasterizationScale;
            AppWindow appWindow = WindowHelper.CurrentAppWindow;
            MainWindow mainWindow = WindowHelper.CurrentMainWindow;

            mainWindow.ExtendsContentIntoTitleBar = true;
            mainWindow.SetTitleBar(appTitleBar);

            appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            appTitleBar.Height = appWindow.TitleBar.Height;

            ColumnDefinition firstColumn = appTitleBar.ColumnDefinitions.FirstOrDefault();
            ColumnDefinition lastColumn = appTitleBar.ColumnDefinitions.LastOrDefault();

            if (firstColumn == null || lastColumn == null) throw new InvalidOperationException("AppTitleBar doesn't contain enough columns.");

            firstColumn.Width = new GridLength(appWindow.TitleBar.LeftInset / scaleAdjustment);
            lastColumn.Width = new GridLength(appWindow.TitleBar.RightInset / scaleAdjustment);
        }
    }
}
