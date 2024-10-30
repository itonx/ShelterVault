using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
 
namespace ShelterVault.Shared.Behaviors
{
    public class InteractiveAppTitleBarBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement frameworkElementNonClientArea = sender as FrameworkElement;
            Window window = (Application.Current as App)?.m_window;
            if (window == null || frameworkElementNonClientArea == null) return;
            setupElement(frameworkElementNonClientArea, window);
        }

        private void setupElement(FrameworkElement frameworkElement, Window window)
        {
            GeneralTransform transformFrameworkElement = frameworkElement.TransformToVisual(null);
            Rect bounds = transformFrameworkElement.TransformBounds(new Rect(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight));
            double scale = window.Content.XamlRoot.RasterizationScale;

            RectInt32 transparentRect = new (
                _X: (int)Math.Round(bounds.X * scale),
                _Y: (int)Math.Round(bounds.Y * scale),
                _Width: (int)Math.Round(bounds.Width * scale),
                _Height: (int)Math.Round(bounds.Height * scale)
            );
            RectInt32[] rectArr = { transparentRect };
            setupClickRegion(rectArr, window);
        }

        private void setupClickRegion(RectInt32[] rects, Window window)
        {
            InputNonClientPointerSource nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(window.AppWindow.Id);
            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rects);
        }
    }
}
