using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace ShelterVault.ViewModels
{
    internal partial class NavigationViewModel : ObservableObject
    {
        private readonly ICredentialsReaderManager _credentialsReaderManager;

        [ObservableProperty]
        private IList<CredentialsViewItem> _credentials;
        [ObservableProperty]
        private object _selectedMenuItem;

        public NavigationViewModel(ICredentialsReaderManager credentialsReaderManager)
        {
            RegisterMessages();
            _credentialsReaderManager = credentialsReaderManager;
            Credentials = _credentialsReaderManager.GetAllCredentials().ToList();
            SelectedMenuItem = Shared.Enums.ShelterVaultPage.HOME.ToString();
        }

        private void RegisterMessages()
        {
            WeakReferenceMessenger.Default.Register<NavigationViewModel, ShowPageRequestMessage>(this, (receiver, message) =>
            {
                if (message.Value == Shared.Enums.ShelterVaultPage.HOME) receiver.SelectedMenuItem = Shared.Enums.ShelterVaultPage.HOME.ToString();
            });
            WeakReferenceMessenger.Default.Register<NavigationViewModel, RefreshCredentialListRequestMessage>(this, (receiver, message) =>
            {
                if (message.Value)
                {
                    receiver.Credentials = receiver._credentialsReaderManager.GetAllCredentials().ToList();
                }
            });
            WeakReferenceMessenger.Default.Register<NavigationViewModel, SelectCredentialRequestMessage>(this, (receiver, message) =>
            {
                if (message.Value != null)
                {
                    CredentialsViewItem selectTarget = Credentials.FirstOrDefault(c => c.UUID.Equals(message.Value.UUID));
                    receiver.SelectedMenuItem = selectTarget;
                }
            });
        }
    }
}
