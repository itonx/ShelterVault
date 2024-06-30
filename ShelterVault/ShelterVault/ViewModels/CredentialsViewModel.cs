using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.Models;
using ShelterVault.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    public class CredentialsViewModel : ObservableObject
    {
        private Credential _lastSelectedItem;
        private bool _confirmationInProcess;
        private bool _requestConfirmation => _lastSelectedItem == null || Credentials.First(c => c.UUID == _lastSelectedItem.UUID).Equals(_lastSelectedItem);
        private Credential _selectedCredential;
        public Credential SelectedCredential
        {
            get => _selectedCredential;
            set => SetProperty(ref _selectedCredential, value.Clone());
        }
        public ObservableCollection<Credential> Credentials { get; }
        public IRelayCommand DeleteCredentialCommand { get; }
        public IRelayCommand SelectedCredentialChangedCommand { get; }

        public CredentialsViewModel()
        {
            Credentials = new ObservableCollection<Credential>(ShelterVaultSqliteTool.GetAllCredentials());
            DeleteCredentialCommand = new RelayCommand(DeleteCredential);
            SelectedCredentialChangedCommand = new RelayCommand<object>(SelectedCredentialChanged);
        }

        private async void SelectedCredentialChanged(object parameter)
        {
            /* Pending changes not saved, ask for user confirmation */
            if (!_requestConfirmation && !_confirmationInProcess)
            {
                _confirmationInProcess = true;
                SelectedCredential = Credentials.First(c => c.UUID == _lastSelectedItem.UUID);
                SelectedCredential = _lastSelectedItem;
                bool cancelNewSelection = await UITools.ShowContinueConfirmationDialogAsync("Important", "You have pending changes, do you want to continue?");
                if(!cancelNewSelection) SelectedCredential = (Credential)(parameter as SelectionChangedEventArgs).AddedItems[0];
                _confirmationInProcess = false;
            }
            if(!_confirmationInProcess) _lastSelectedItem = SelectedCredential;
        }

        private async void DeleteCredential()
        {
            if (SelectedCredential == null || string.IsNullOrWhiteSpace(SelectedCredential.UUID)) return;

            if (ShelterVaultSqliteTool.DeleteCredential(SelectedCredential.UUID)) await UITools.ShowConfirmationDialogAsync("Shelter Vault", "Your credential was deleted.", "OK");
            else await UITools.ShowConfirmationDialogAsync("Shelter Vault", "Your credential couldn't be deleted.", "OK");
        }

    }
}
