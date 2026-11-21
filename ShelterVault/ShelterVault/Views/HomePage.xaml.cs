using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ShelterVault.Models;
using ShelterVault.Shared.Messages;
using ShelterVault.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShelterVault.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
            this.DataContext = App.Current.Services.GetService<HomeViewModel>();
        }

        private void ItemContainer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (
                sender is ItemContainer container
                && CredentialList.SelectedItem is CredentialsViewItem credentialsViewItem
            )
            {
                WeakReferenceMessenger.Default.Send(
                    new LoadCredentialsMessage(
                        LoadPagePayload.Create(
                            Shared.Enums.ShelterVaultPage.CREDENTIALS,
                            credentialsViewItem
                        )
                    )
                );
            }
        }
    }
}
