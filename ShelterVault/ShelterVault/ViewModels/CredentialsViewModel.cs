﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.Models;
using ShelterVault.Tools;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    public class CredentialsViewModel : ObservableObject
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Credential _lastSelectedItem;
        private bool _confirmationInProcess;
        private bool _requestConfirmation => _lastSelectedItem == null || Credentials.First(c => c.UUID == _lastSelectedItem.UUID).Equals(_lastSelectedItem);
        private Credential _selectedCredential;
        public Credential SelectedCredential
        {
            get => _selectedCredential;
            set
            {
                if (!_confirmationInProcess && value != null && value.Password == null)
                {
                    value.Password = EncryptionTool.DecryptAes(Convert.FromBase64String(value.EncryptedPassword), UITools.GetMasterKey(), Convert.FromBase64String(value.InitializationVector));
                    value.PasswordConfirmation = value.Password;
                }
                SetProperty(ref _selectedCredential, value == null ? null : value.Clone());
            }
        }
        private bool _togglePasswordVisibility;
        public bool TogglePasswordVisibility
        {
            get => _togglePasswordVisibility;
            set => SetProperty(ref _togglePasswordVisibility, value);
        }
        private ObservableCollection<Credential> _credentials;
        public ObservableCollection<Credential> Credentials
        {
            get => _credentials;
            set => SetProperty(ref _credentials, value);
        }
        public IRelayCommand DeleteCredentialCommand { get; }
        public IRelayCommand SetClipboardCommand { get; }
        public IRelayCommand ShowPasswordCommand { get; }
        public IRelayCommand SaveCredentialChangesCommand { get; }
        public IRelayCommand SelectedCredentialChangedCommand { get; }

        public CredentialsViewModel()
        {
            Credentials = new ObservableCollection<Credential>(ShelterVaultSqliteTool.GetAllCredentials());
            DeleteCredentialCommand = new RelayCommand(DeleteCredential);
            SetClipboardCommand = new RelayCommand(SetClipboard);
            ShowPasswordCommand = new RelayCommand(ShowPassword);
            SaveCredentialChangesCommand = new RelayCommand(SaveCredentialChanges);
            SelectedCredentialChangedCommand = new RelayCommand<object>(SelectedCredentialChanged);
        }

        private void SetClipboard()
        {
            if(_cancellationTokenSource != null)
            {
                try
                {
                    _cancellationTokenSource.Cancel();
                }
                finally 
                {
                    _cancellationTokenSource.Dispose();
                }
            }

            CancellationTokenSource tokenCancellation = new CancellationTokenSource();
            CancellationToken ct = tokenCancellation.Token;
            _cancellationTokenSource = tokenCancellation;

            SelectedCredential.Password.SendToClipboard();

            Task.Run(async () =>
            {
                await Task.Delay(5000);
                ct.ThrowIfCancellationRequested();
                "ShelterVault".SendToClipboard();
            }, tokenCancellation.Token);

        }

        private void ShowPassword()
        {
            TogglePasswordVisibility = !TogglePasswordVisibility;
        }

        private async void SaveCredentialChanges()
        {
            StringBuilder err = new StringBuilder();

            if (SelectedCredential.IsNewCredentialValid(err))
            {
                Credential credential = Credentials.First(c => c.UUID == SelectedCredential.UUID);
                Credential credentialUpdated = SelectedCredential;
                if(credential.Password != SelectedCredential.Password)
                {
                    (byte[], byte[]) encryptedValues = EncryptionTool.EncryptAes(SelectedCredential.Password, UITools.GetMasterKey());
                    credentialUpdated = SelectedCredential.GetUpdatedCredentialValues(encryptedValues);
                }

                bool updated = ShelterVaultSqliteTool.UpdateCredential(credentialUpdated);
                if (updated)
                {
                    Credential credentialUpdatedInView = Credentials.First(c => c.UUID == credentialUpdated.UUID);
                    Credentials[Credentials.IndexOf(credentialUpdatedInView)] = credentialUpdated;
                    SelectedCredential = null;
                    SelectedCredential = Credentials.First(c => c.UUID == credentialUpdated.UUID);
                    await UITools.ShowConfirmationDialogAsync("Important", "Chages were saved.");
                }
                else await UITools.ShowConfirmationDialogAsync("Important", "Your credential could't be saved.");
            }
            else
            {
                await UITools.ShowConfirmationDialogAsync("Important", err.ToString());
            }
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
