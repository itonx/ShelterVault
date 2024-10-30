
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
        private static AppWindow _currentAppWindow;
        private static MainWindow _mainWindow;
        public static AppWindow CurrentAppWindow => GetAppWindow();
        public static MainWindow CurrentMainWindow => GetMainWindow();

        private static AppWindow GetAppWindow()
        {
            if (_currentAppWindow != null) return _currentAppWindow;
            IntPtr hWnd = WindowNative.GetWindowHandle(CurrentMainWindow);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _currentAppWindow = AppWindow.GetFromWindowId(wndId);
            return _currentAppWindow;
        }

        private static MainWindow GetMainWindow()
        {
            if (_mainWindow != null) return _mainWindow;
            _mainWindow = (Application.Current as App)?.m_window as MainWindow;
            return _mainWindow;
        }
    }
}
