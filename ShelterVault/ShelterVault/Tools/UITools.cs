using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using ShelterVault.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using System.IO;
using ShelterVault.ViewModels;
using System.Runtime.InteropServices;
using Windows.Storage;

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
            MainWindow mainWindow = GetMainWindow();
            if (mainWindow != null) 
            {
                mainWindow.AppContent.Content = new MasterKeyConfirmationView();
            } 
        }

        public static void LoadCredentialsView(byte[] password, byte[] salt)
        {
            MainWindow mainWindow = GetMainWindow();
            if(mainWindow != null)
            {
                mainWindow.ThemeToggle.Visibility = Visibility.Collapsed;
                MainWindowViewModel viewModel = mainWindow.WindowContent.DataContext as MainWindowViewModel;
                viewModel.ProtectMasterKey(password, salt);
                mainWindow.AppContent.Content = new CredentialsView();
            }
        }

        public static byte[] GetMasterKey()
        {
            MainWindow mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                MainWindowViewModel viewModel = mainWindow.WindowContent.DataContext as MainWindowViewModel;
                return viewModel.GetMasterKeyUnprotected();
            }

            return Array.Empty<byte>();
        }

        public static byte[] GetMasterKeySalt()
        {
            MainWindow mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                MainWindowViewModel viewModel = mainWindow.WindowContent.DataContext as MainWindowViewModel;
                return viewModel.GetMasterKeySaltUnprotected();
            }

            return Array.Empty<byte>();
        }

        public static void LoadInitialView()
        {
            MainWindow mainWindow = GetMainWindow();
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
            MainWindow mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                mainWindow.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "icon.ico"));
            }
        }

        public static void LoadTheme()
        {
            MainWindow mainWindow = GetMainWindow();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string theme = localSettings.Values[SETTINGS_THEME_KEY] as string;
            
            if (mainWindow != null)
            {
                ElementTheme elementTheme = theme == null ? ElementTheme.Default : (ElementTheme)Enum.Parse(typeof(ElementTheme), theme);

                if(elementTheme == ElementTheme.Default)
                {
                    bool isDarkMode = PInvoke.ShouldSystemUseDarkMode();
                    mainWindow.ThemeToggle.IsChecked = !isDarkMode;

                }
                else
                {
                    ((FrameworkElement)mainWindow.Content).RequestedTheme = elementTheme;
                    mainWindow.ThemeToggle.IsChecked = elementTheme == ElementTheme.Light;
                }
            }
        }

        public static void ChangeTheme(bool autoFlip = true)
        {
            MainWindow mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                if(!autoFlip) mainWindow.ThemeToggle.IsChecked = !mainWindow.ThemeToggle.IsChecked;
                ((FrameworkElement)mainWindow.Content).RequestedTheme = mainWindow.ThemeToggle.IsChecked == true ? ElementTheme.Light : ElementTheme.Dark;
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                string currentTheme = ((FrameworkElement)mainWindow.Content).RequestedTheme.ToString();
                localSettings.Values[SETTINGS_THEME_KEY] = currentTheme;
            }

        }

        public static async Task ShowSpinner()
        {
            MainWindow mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                MainWindowViewModel viewModel = mainWindow.WindowContent.DataContext as MainWindowViewModel;
                viewModel.IsProgressBarVisible = true;
            }
            await Task.Delay(50);
        }

        public static async Task HideSpinner()
        {
            MainWindow mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                MainWindowViewModel viewModel = mainWindow.WindowContent.DataContext as MainWindowViewModel;
                viewModel.IsProgressBarVisible = false;
            }
            await Task.Delay(0);
        }

        private static MainWindow GetMainWindow()
        {
            return (Application.Current as App)?.m_window as MainWindow;
        }

        private static string SETTINGS_THEME_KEY = "ShelterVault.Theme";
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
