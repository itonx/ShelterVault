using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Helpers;
using ShelterVault.Shared.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.Shared.Behaviors
{
    public class MaximizeWindowAtStartupBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            MainWindow mainWindow = WindowHelper.CurrentMainWindow;
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
            PInvoke.MaximizeWindow(hWnd);
        }
    }
}
