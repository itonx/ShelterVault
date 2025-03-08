using Desktiny.UI.Tools;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;
using System.Linq;

namespace Desktiny.UI.Behaviors
{
    public class EnableCustomAppTitleBarBehavior : Behavior<WinContainer>
    {
        private Grid _appTitleBar;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _appTitleBar = WinUI3Helper.FindChildElementByName(AssociatedObject, "AppTitleBar") as Grid;
            _appTitleBar.Loaded += AppTitleBar_Loaded;
            _appTitleBar.SizeChanged += AppTitleBar_SizeChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            if (_appTitleBar == null) return;
            _appTitleBar.Loaded -= AppTitleBar_Loaded;
            _appTitleBar.SizeChanged -= AppTitleBar_SizeChanged;
        }

        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_appTitleBar == null) return;
            AppWindow appWindow = AssociatedObject.AppWindow;
            double scale = _appTitleBar.XamlRoot.RasterizationScale;
            OverlappedPresenter windowPresenter = appWindow.Presenter as OverlappedPresenter;
            if (windowPresenter.State == OverlappedPresenterState.Maximized)
            {
                _appTitleBar.Height = appWindow.TitleBar.Height;
            }
            else
            {
                double newHeight = appWindow.TitleBar.Height - 1;
                _appTitleBar.Height = newHeight < 0 ? 0 : newHeight;
            }
            AdjustCaptionColumns(scale);
        }

        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            AppWindow appWindow = AssociatedObject.AppWindow;
            Window mainWindow = AssociatedObject.MainWindow;

            mainWindow.ExtendsContentIntoTitleBar = true;
            mainWindow.SetTitleBar(_appTitleBar);

            appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            _appTitleBar.Height = appWindow.TitleBar.Height;
        }

        private void AdjustCaptionColumns(double scale)
        {
            AppWindow appWindow = AssociatedObject.AppWindow;
            ColumnDefinition firstColumn = _appTitleBar.ColumnDefinitions.FirstOrDefault();
            ColumnDefinition lastColumn = _appTitleBar.ColumnDefinitions.LastOrDefault();

            if (firstColumn == null || lastColumn == null) throw new InvalidOperationException("AppTitleBar doesn't contain enough columns.");

            firstColumn.Width = new GridLength(appWindow.TitleBar.LeftInset / scale);
            lastColumn.Width = new GridLength(appWindow.TitleBar.RightInset + 1 / scale);
        }
    }
}
