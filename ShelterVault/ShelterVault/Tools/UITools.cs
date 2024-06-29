using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using ShelterVault.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Tools
{
    public static class UITools
    {
        private static ContentDialog BuildDialog(string title, string message, string primaryButtonText = "Close")
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = ((Application.Current as App)?.m_window as MainWindow).Content.XamlRoot;
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

        public static async Task<bool> ShowContinueConfirmationDialogAsync(string title, string message, string primaryButtonText = "No", string secondaryButtonText = "Yes")
        {
            ContentDialog dialog = BuildDialog(title, message, primaryButtonText);
            dialog.SecondaryButtonText = secondaryButtonText;
            ContentDialogResult result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        public static void LoadMasterKeyConfirmationView()
        {
            MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
            mainWindow?.LoadMasterKeyConfirmationView();
        }

        public static void LoadCredentialsView(byte[] password)
        {
            MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
            mainWindow?.LoadCredentialsView(password);
        }

        public static byte[] GetMasterKey()
        {
            MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
            return mainWindow?.GetMasterKey();
        }
    }
}
