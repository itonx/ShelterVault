using System;
using System.Runtime.InteropServices;

namespace Desktiny.UI.Tools
{
    public class PInvoke
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        static extern bool ShouldSystemUseDarkMode();

        public enum WindowShowStyle : uint
        {
            MAXIMIZED = 3,
        }

        public static void MaximizeWindow(IntPtr hWnd)
        {
            ShowWindow(hWnd, WindowShowStyle.MAXIMIZED);
        }

        public static bool UseDarkMode => ShouldSystemUseDarkMode();
    }
}
