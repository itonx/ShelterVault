using Microsoft.UI.Xaml;
using System.Security.Cryptography;
using ShelterVault.Views;
using ShelterVault.Tools;
using Microsoft.UI;
using System;
using Windows.ApplicationModel;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShelterVault
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private byte[] _encryptedPassword;

        public MainWindow()
        {
            this.InitializeComponent();
            LoadInitialView();
            LoadIcon();
        }

        public byte[] GetMasterKey()
        {
            return ProtectedData.Unprotect(_encryptedPassword, null, DataProtectionScope.CurrentUser);
        }

        public void LoadMasterKeyConfirmationView()
        {
            this.AppContent.Content = new MasterKeyConfirmationView();
        }

        public void LoadCredentialsView(byte[] password)
        {
            _encryptedPassword = ProtectedData.Protect(password, null, DataProtectionScope.CurrentUser);
            this.AppContent.Content = new CredentialsView();
        }

        public void LoadInitialView()
        {
            if(ShelterVaultSqliteTool.DBExists()) this.AppContent.Content = new MasterKeyConfirmationView();
            else this.AppContent.Content = new CreateMasterKeyView();
        }

        private void LoadIcon()
        {
            this.AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "icon.ico"));
        }
    }
}
