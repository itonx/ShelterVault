using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ShelterVault.Models;
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
    public sealed partial class CredentialsView : UserControl
    {
        public CredentialsView()
        {
            this.InitializeComponent();
            /*this.DataContext = this;
            this.CredentialList.ItemsSource = new List<Credential>()
            {
                new Credential()
                {
                    Title = "Title 1",
                    Notes = "Note 1"
                },
                new Credential()
                {
                    Title = "Title 2",
                    Notes = "Note 2"
                }
            };*/
        }

        /*private void CredentialList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Credential selectedCredential = (sender as ListView).SelectedItem as Credential;

            if (selectedCredential == null) return;

            this.SelectedTitle.Text = selectedCredential.Title;
            this.SelectedUsername.Text = selectedCredential.Username;
            this.SelectedEncriptedPassword.Text = selectedCredential.EncryptedPassword;
            this.SelectedUrl.Text = selectedCredential.Url;
            this.SelectedNotes.Document.Selection.Text = selectedCredential.Notes;
        }*/
    }
}
