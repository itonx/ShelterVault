using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ShelterVault.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShelterVault.Views
{
    public sealed partial class MasterKeyConfirmationView : UserControl
    {
        public MasterKeyConfirmationView()
        {
            this.InitializeComponent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            this.password.Password = string.Empty;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShelterVaultSqliteTool.IsMasterKey(this.password.Password))
            {
                MainWindow mainWindow = (Application.Current as App)?.m_window as MainWindow;
                mainWindow?.LoadCredentialsView();
            }
        }
    }
}
