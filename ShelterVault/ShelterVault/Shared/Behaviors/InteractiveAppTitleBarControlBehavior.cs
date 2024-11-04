using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
 
namespace ShelterVault.Shared.Behaviors
{
    public class InteractiveAppTitleBarControlBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            MainWindow mainWindow = WindowHelper.CurrentMainWindow;
            mainWindow.AppTitleBar.SizeChanged -= AppTitleBar_SizeChanged;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            MainWindow mainWindow = WindowHelper.CurrentMainWindow;
            setupElement(AssociatedObject);
            mainWindow.AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
        }

        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            setupElement(AssociatedObject);
        }

        private void setupElement(FrameworkElement interactiveControl)
        {
            MainWindow window = WindowHelper.CurrentMainWindow;
            if (window == null || interactiveControl == null) return;

            double scale = window.AppTitleBar.XamlRoot.RasterizationScale;

            GeneralTransform transformInteractiveControl = interactiveControl.TransformToVisual(null);
            Rect boundsInteractiveControl = transformInteractiveControl.TransformBounds(new Rect(0, 0, interactiveControl.ActualWidth, interactiveControl.ActualHeight));

            RectInt32 transparentRect = GetRect(boundsInteractiveControl, scale);
            RectInt32[] rectArr = { transparentRect };
            setupClickRegion(rectArr, window);
        }

        private void setupClickRegion(RectInt32[] rects, Window window)
        {
            InputNonClientPointerSource nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(window.AppWindow.Id);
            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rects);
        }

        private RectInt32 GetRect(Rect bounds, double scale)
        {
            return new (
                _X: (int)Math.Round(bounds.X * scale),
                _Y: (int)Math.Round(bounds.Y * scale),
                _Width: (int)Math.Round(bounds.Width * scale),
                _Height: (int)Math.Round(bounds.Height * scale)
            );
        }
    }
}
