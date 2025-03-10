
using Desktiny.UI.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;

namespace Desktiny.UI.Tools
{
    public static class WindowHelper
    {
        private static AppWindow _currentAppWindow;
        private static Window _mainWindow;
        public static AppWindow CurrentAppWindow => GetAppWindow();
        public static Window CurrentMainWindow => GetMainWindow();

        private static AppWindow GetAppWindow()
        {
            if (_currentAppWindow != null) return _currentAppWindow;
            Window mainWindow = CurrentMainWindow;
            if (mainWindow == null) return null;
            IntPtr hWnd = WindowNative.GetWindowHandle(mainWindow);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            _currentAppWindow = AppWindow.GetFromWindowId(wndId);
            return _currentAppWindow;
        }

        private static Window GetMainWindow()
        {
            if (_mainWindow != null) return _mainWindow;
            _mainWindow = (Application.Current as IAppWindow)?.MainWindow;
            return _mainWindow;
        }
    }
}
