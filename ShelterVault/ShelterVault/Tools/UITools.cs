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

        public static void LoadMasterKeyConfirmationView(byte[] password)
        {
            MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
            mainWindow?.LoadMasterKeyConfirmationView(password);
        }

        public static void LoadCredentialsView()
        {
            MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
            mainWindow?.LoadCredentialsView();
        }
    }
}
