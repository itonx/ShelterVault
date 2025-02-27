
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
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
            MainWindow mainWindow = CurrentMainWindow;
            if (mainWindow == null) return null;
            IntPtr hWnd = WindowNative.GetWindowHandle(mainWindow);
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
