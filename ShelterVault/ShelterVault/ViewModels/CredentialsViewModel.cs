using CommunityToolkit.Mvvm.ComponentModel;
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
                if (!_confirmationInProcess && value != null && value.Password == null && value.EncryptedPassword != null)
                {
                    value.Password = EncryptionTool.DecryptAes(Convert.FromBase64String(value.EncryptedPassword), UITools.GetMasterKey(), Convert.FromBase64String(value.InitializationVector));
                    value.PasswordConfirmation = value.Password;
                }
                SetProperty(ref _selectedCredential, value == null ? null : value.Clone());
            }
        }
        private bool _togglePasswordVisibility;
        public bool ShowPassword
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
        private CredentialsViewModelState _state;
        public CredentialsViewModelState State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }
        public IRelayCommand NewCredentialCommand { get; }
        public IRelayCommand CancelCredentialCommand { get; }
        public IRelayCommand DeleteCredentialCommand { get; }
        public IRelayCommand SetClipboardCommand { get; }
        public IRelayCommand ShowPasswordCommand { get; }
        public IRelayCommand SaveCredentialChangesCommand { get; }
        public IRelayCommand SelectedCredentialChangedCommand { get; }

        public CredentialsViewModel()
        {
            State = CredentialsViewModelState.Default;
            ShowPassword = false;
            Credentials = new ObservableCollection<Credential>(ShelterVaultSqliteTool.GetAllCredentials());
            NewCredentialCommand = new RelayCommand(OnNewCredential);
            CancelCredentialCommand = new RelayCommand(OnCancelCredential);
            DeleteCredentialCommand = new RelayCommand(OnDeleteCredential);
            SetClipboardCommand = new RelayCommand(OnSetClipboard);
            ShowPasswordCommand = new RelayCommand(OnShowPassword);
            SaveCredentialChangesCommand = new RelayCommand(OnSaveCredentialChanges);
            SelectedCredentialChangedCommand = new RelayCommand<object>(OnSelectedCredentialChanged);
        }

        private void OnCancelCredential()
        {
            State = CredentialsViewModelState.Default;
            SelectedCredential = null;
        }

        private void OnNewCredential()
        {
            State = CredentialsViewModelState.Adding;
            Credential newCredential = new Credential();
            newCredential.Password = newCredential.PasswordConfirmation = string.Empty;
            SelectedCredential = null;
            SelectedCredential = newCredential;
            ShowPassword = false;
        }

        private void OnSetClipboard()
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

        private void OnShowPassword()
        {
            ShowPassword = !ShowPassword;
        }

        private async void OnSaveCredentialChanges()
        {
            StringBuilder err = new StringBuilder();

            if (SelectedCredential.IsUpdatedCredentialValid(err))
            {
                if (State == CredentialsViewModelState.Default)
                {
                    Credential credential = Credentials.First(c => c.UUID == SelectedCredential.UUID);
                    Credential credentialUpdated = SelectedCredential;
                    if (credential.Password != SelectedCredential.Password)
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
                    else await UITools.ShowConfirmationDialogAsync("Important", "Your credential could't be updated.");
                }
                else
                {
                    Credential newCredential = SelectedCredential;
                    (byte[], byte[]) encryptedValues = EncryptionTool.EncryptAes(SelectedCredential.Password, UITools.GetMasterKey());
                    newCredential = SelectedCredential.GetUpdatedCredentialValues(encryptedValues);

                    bool inserted = ShelterVaultSqliteTool.InsertCredential(newCredential);
                    if (inserted)
                    {
                        Credentials.Add(newCredential);
                        SelectedCredential = null;
                        SelectedCredential = Credentials.First(c => c.UUID == newCredential.UUID);
                        await UITools.ShowConfirmationDialogAsync("Important", "Your credential was saved.");
                        State = CredentialsViewModelState.Default;
                    }
                    else await UITools.ShowConfirmationDialogAsync("Important", "Your credential could't be saved.");
                }
            }
            else
            {
                await UITools.ShowConfirmationDialogAsync("Important", err.ToString());
            }
        }

        private async void OnSelectedCredentialChanged(object parameter)
        {
            /* Pending changes not saved, ask for user confirmation before loosing changes */
            if (!_requestConfirmation && !_confirmationInProcess)
            {
                _confirmationInProcess = true;
                SelectedCredential = Credentials.First(c => c.UUID == _lastSelectedItem.UUID);
                SelectedCredential = _lastSelectedItem;
                bool cancelNewSelection = await UITools.ShowContinueConfirmationDialogAsync("Important", "You have pending changes, do you want to continue?");
                if(!cancelNewSelection) SelectedCredential = (Credential)(parameter as SelectionChangedEventArgs).AddedItems[0];
                _confirmationInProcess = false;
            }
            if (!_confirmationInProcess)
            {
                ShowPassword = false;
                _lastSelectedItem = SelectedCredential;
            }
        }

        private async void OnDeleteCredential()
        {
            if (SelectedCredential == null || string.IsNullOrWhiteSpace(SelectedCredential.UUID)) return;
            string uuid = SelectedCredential.UUID;
            if (ShelterVaultSqliteTool.DeleteCredential(uuid))
            {
                SelectedCredential = null;
                SelectedCredential = new Credential();
                SelectedCredential.Password = SelectedCredential.PasswordConfirmation = string.Empty;
                Credential credential = Credentials.First(c => c.UUID == uuid);
                Credentials.Remove(credential);
                await UITools.ShowConfirmationDialogAsync("Shelter Vault", "Your credential was deleted.", "OK");
            }
            else await UITools.ShowConfirmationDialogAsync("Shelter Vault", "Your credential couldn't be deleted.", "OK");
        }

    }
}
