using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Helpers;
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

    class PInvoke
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        public static extern bool ShouldSystemUseDarkMode();

        public enum WindowShowStyle : uint
        {
            MAXIMIZED = 3,
        }

        public static void MaximizeWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, WindowShowStyle.MAXIMIZED);
        }
    }
}
