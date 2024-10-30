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
            AssociatedObject.SizeChanged += AssociatedObject_SizeChanged;
        }

        private void AssociatedObject_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid appTitleBar = (sender as Grid);
            AppWindow appWindow = WindowHelper.CurrentAppWindow;
            double scale = WindowHelper.CurrentMainWindow.AppTitleBar.XamlRoot.RasterizationScale;
            OverlappedPresenter windowPresenter = appWindow.Presenter as OverlappedPresenter;
            if (windowPresenter.State == OverlappedPresenterState.Maximized)
            {
                appTitleBar.Height = appWindow.TitleBar.Height;
            }
            else
            {
                double newHeight = appWindow.TitleBar.Height - 1;
                appTitleBar.Height = newHeight < 0 ? 0 : newHeight;
            }
            AdjustCaptionColumns(scale);
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            Grid appTitleBar = (sender as Grid);
            appTitleBar.Loaded -= AssociatedObject_Loaded;

            AppWindow appWindow = WindowHelper.CurrentAppWindow;
            MainWindow mainWindow = WindowHelper.CurrentMainWindow;

            mainWindow.ExtendsContentIntoTitleBar = true;
            mainWindow.SetTitleBar(appTitleBar);

            appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            appTitleBar.Height = appWindow.TitleBar.Height;
        }

        private void AdjustCaptionColumns(double scale)
        {
            AppWindow appWindow = WindowHelper.CurrentAppWindow;
            Grid appTitleBar = WindowHelper.CurrentMainWindow.AppTitleBar;
            ColumnDefinition firstColumn = appTitleBar.ColumnDefinitions.FirstOrDefault();
            ColumnDefinition lastColumn = appTitleBar.ColumnDefinitions.LastOrDefault();

            if (firstColumn == null || lastColumn == null) throw new InvalidOperationException("AppTitleBar doesn't contain enough columns.");

            firstColumn.Width = new GridLength(appWindow.TitleBar.LeftInset / scale);
            lastColumn.Width = new GridLength(appWindow.TitleBar.RightInset + 1 / scale);
        }
    }
}
