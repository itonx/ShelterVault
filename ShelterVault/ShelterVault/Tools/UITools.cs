using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using ShelterVault.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using System.IO;
using ShelterVault.ViewModels;
using System.Runtime.InteropServices;

namespace ShelterVault.Tools
{
    public static class UITools
    {
        private static ContentDialog BuildDialog(string title, string message, string primaryButtonText = "Close")
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = ((Application.Current as App)?.m_window as MainWindow)?.Content.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = title;
            dialog.PrimaryButtonText = primaryButtonText;
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new ShelterVaultMessageView(message);

            return dialog;
        }

        public static async Task ShowConfirmationDialogAsync(string title, string message, string primaryButtonText = "Close")
        {
            ContentDialog dialog = BuildDialog(title, message, primaryButtonText);
            await dialog.ShowAsync();
        }

        public static async Task<bool> ShowContinueConfirmationDialogAsync(string title, string message, string primaryButtonText = "No", string secondaryButtonText = "Yes", ContentDialogResult expectedResult = ContentDialogResult.Primary)
        {
            ContentDialog dialog = BuildDialog(title, message, primaryButtonText);
            dialog.SecondaryButtonText = secondaryButtonText;
            ContentDialogResult result = await dialog.ShowAsync();
            return result == expectedResult;
        }

        public static void LoadMasterKeyConfirmationView()
        {
            MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
            if (mainWindow != null) 
            {
                mainWindow.AppContent.Content = new MasterKeyConfirmationView();
            } 
        }

        public static void LoadCredentialsView(byte[] password)
        {
            MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
            if(mainWindow != null)
            {
                MainWindowViewModel viewModel = mainWindow.WindowContent.DataContext as MainWindowViewModel;
                viewModel.ProtectMasterKey(password);
                mainWindow.AppContent.Content = new CredentialsView();
            }
        }

        public static byte[] GetMasterKey()
        {
            MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
            if (mainWindow != null)
            {
                MainWindowViewModel viewModel = mainWindow.WindowContent.DataContext as MainWindowViewModel;
                return viewModel.GetMasterKeyUnprotected();
            }

            return Array.Empty<byte>();
        }

        public static void LoadInitialView()
        {
            MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
            if (mainWindow != null)
            {
                IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
                PInvoke.MaximizeWindow(hWnd);
                if (ShelterVaultSqliteTool.DBExists()) mainWindow.AppContent.Content = new MasterKeyConfirmationView();
                else mainWindow.AppContent.Content = new CreateMasterKeyView();
            }
        }

        public static void LoadAppIcon()
        {
            MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "icon.ico"));
            }
        }
    }

    class PInvoke
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, WindowShowStyle nCmdShow);

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
