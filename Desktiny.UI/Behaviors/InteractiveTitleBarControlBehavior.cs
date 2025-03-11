using Desktiny.UI.Tools;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using System;
using Windows.Foundation;
using Windows.Graphics;

namespace Desktiny.UI.Behaviors
{
    public class InteractiveTitleBarControlBehavior : Behavior<FrameworkElement>
    {
        private Grid _appTitleBar;
        private Window _mainWindow;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            _appTitleBar.SizeChanged -= AppTitleBar_SizeChanged;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            XamlRoot root = AssociatedObject.XamlRoot;
            WinContainer winContainer = root.Content as WinContainer;
            _appTitleBar = winContainer.GetAppTitleBar();
            _mainWindow = winContainer.MainWindow;

            setupElement(AssociatedObject);
            _appTitleBar.SizeChanged += AppTitleBar_SizeChanged;
        }

        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            setupElement(AssociatedObject);
        }

        private void setupElement(FrameworkElement interactiveControl)
        {
            if (_mainWindow == null || interactiveControl == null) return;

            double scale = _appTitleBar.XamlRoot.RasterizationScale;

            GeneralTransform transformInteractiveControl = interactiveControl.TransformToVisual(null);
            Rect boundsInteractiveControl = transformInteractiveControl.TransformBounds(new Rect(0, 0, interactiveControl.ActualWidth, interactiveControl.ActualHeight));

            RectInt32 transparentRect = GetRect(boundsInteractiveControl, scale);
            RectInt32[] rectArr = { transparentRect };
            setupClickRegion(rectArr);
        }

        private void setupClickRegion(RectInt32[] rects)
        {
            InputNonClientPointerSource nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(_mainWindow.AppWindow.Id);
            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rects);
        }

        private RectInt32 GetRect(Rect bounds, double scale)
        {
            return new(
                _X: (int)Math.Round(bounds.X * scale),
                _Y: (int)Math.Round(bounds.Y * scale),
                _Width: (int)Math.Round(bounds.Width * scale),
                _Height: (int)Math.Round(bounds.Height * scale)
            );
        }
    }
}
