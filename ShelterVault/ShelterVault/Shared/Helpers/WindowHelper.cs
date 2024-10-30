
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT.Interop;

namespace ShelterVault.Shared.Helpers
{
    public static class WindowHelper
    {
        public static AppWindow CurrentAppWindow => GetAppWindow();
        public static MainWindow CurrentMainWindow => (Application.Current as App)?.m_window as MainWindow;

        public static AppWindow GetAppWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(CurrentMainWindow);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }
    }
}
