using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Text;
using System.Diagnostics;
using ShelterVault.Views;
using ShelterVault.Tools;

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
        }

        private byte[] DecryptPassword(byte[] encryptedData)
        {
            return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);
        }
        public void TestEncryptDecrypt()
        {
            string original = "Here is some data to encrypt!";
            Debug.WriteLine($"Original: {original}");
            byte[] passwordDecrypted = DecryptPassword(_encryptedPassword);
            var encrypted = EncryptionTool.EncryptAes(ref original, passwordDecrypted);
            string decrypted = EncryptionTool.DecryptAes(encrypted.Item1, passwordDecrypted, encrypted.Item2);

            Debug.WriteLine($"Encrypted (b64-encode): {Convert.ToBase64String(encrypted.Item1)}");
            Debug.WriteLine($"Decrypted: {decrypted}");
        }

        public void LoadMasterKeyConfirmationView()
        {
            this.AppContent.Content = new MasterKeyConfirmationView();
        }

        public void LoadCredentialsView()
        {
            this.AppContent.Content = new CredentialsView();
        }

        private void LoadInitialView()
        {
            if(ShelterVaultSqliteTool.DBExists()) this.AppContent.Content = new MasterKeyConfirmationView();
            else this.AppContent.Content = new CreateMasterKeyView();
        }
    }
}
