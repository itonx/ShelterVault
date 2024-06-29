using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private Credential _selectedCredential;
        public Credential SelectedCredential
        {
            get
            {
                return _selectedCredential;
            }
            set
            {
                if(CanLoadNewSelectedCredential) SetProperty(ref _selectedCredential, value.Clone());
                else if(UITools.ShowContinueConfirmationDialogAsync("Important", "You have pending changes, do you want to continue?").GetAwaiter().GetResult()) SetProperty(ref _selectedCredential, value);
            }
        }
        public ObservableCollection<Credential> Credentials { get; }
        public IRelayCommand DeleteCredentialCommand { get; }

        public CredentialsViewModel()
        {
            Credentials = new ObservableCollection<Credential>(ShelterVaultSqliteTool.GetAllCredentials());
            DeleteCredentialCommand = new RelayCommand(DeleteCredential);
        }

        private async void DeleteCredential()
        {
            if (SelectedCredential == null || string.IsNullOrWhiteSpace(SelectedCredential.UUID)) return;

            if (ShelterVaultSqliteTool.DeleteCredential(SelectedCredential.UUID)) await UITools.ShowConfirmationDialogAsync("Shelter Vault", "Your credential was deleted.", "OK");
            else await UITools.ShowConfirmationDialogAsync("Shelter Vault", "Your credential couldn't be deleted.", "OK");
        }

        private bool CanLoadNewSelectedCredential => _selectedCredential != null && Credentials.First(c => c.UUID == _selectedCredential.UUID).Equals(_selectedCredential);
    }
}
