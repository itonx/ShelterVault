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
    public partial class CredentialsViewModel : ObservableObject
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Credential _lastSelectedItem;
        private bool _confirmationInProcess;
        private bool _requestConfirmation => _lastSelectedItem != null && !Credentials.First(c => c.UUID == _lastSelectedItem.UUID).Equals(_lastSelectedItem);
        private Credential _selectedCredential;
        public Credential SelectedCredential
        {
            get => _selectedCredential;
            set
            {
                if (!_confirmationInProcess && value != null && value.Password == null && value.EncryptedPassword != null)
                {
                    value.Password = EncryptionTool.DecryptAes(Convert.FromBase64String(value.EncryptedPassword), UITools.GetMasterKey(), Convert.FromBase64String(value.InitializationVector), UITools.GetMasterKeySalt());
                    value.PasswordConfirmation = value.Password;
                }
                SetProperty(ref _selectedCredential, value == null ? null : value.Clone());
            }
        }
        [ObservableProperty]
        private bool _showPassword = false;
        [ObservableProperty]
        private ObservableCollection<Credential> _credentials;
        [ObservableProperty]
        private CredentialsViewModelState _state = CredentialsViewModelState.Adding;
        [ObservableProperty]
        private bool _requestFocusOnFirstField;
        [ObservableProperty]
        private PasswordConfirmationViewModel _passwordRequirementsVM = new PasswordConfirmationViewModel();

        public CredentialsViewModel()
        {
            Credentials = new ObservableCollection<Credential>(ShelterVaultSqliteTool.GetAllCredentials());
            PasswordRequirementsVM.HeaderText = "Password must:";
        }

        [RelayCommand]
        private void ChangeTheme(object obj)
        {
            UITools.ChangeTheme(autoFlip: false);
        }

        [RelayCommand]
        private async Task Home(object obj)
        {
            await ConfirmPendingChangesIfNeeded(() =>
            {
                State = CredentialsViewModelState.Empty;
                SelectedCredential = null;
            });
        }

        [RelayCommand]
        private void CancelCredential()
        {
            State = CredentialsViewModelState.Empty;
            SelectedCredential = null;
        }

        [RelayCommand]
        private async Task NewCredential()
        {
            await ConfirmPendingChangesIfNeeded(() =>
            {
                ShowPassword = false;
                RequestFocusOnFirstField = true;
                State = CredentialsViewModelState.Adding;
                Credential newCredential = new Credential();
                newCredential.Password = newCredential.PasswordConfirmation = string.Empty;
                SelectedCredential = null;
                SelectedCredential = newCredential;
            });
        }

        [RelayCommand]
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

        [RelayCommand]
        private void ChangePasswordVisibility()
        {
            ShowPassword = !ShowPassword;
        }

        [RelayCommand]
        private async Task SaveCredentialChanges()
        {
            try
            {
                if (await PasswordRequirementsVM.AreCredentialsValid(SelectedCredential))
                {
                    await UITools.ShowSpinner();
                    if (State == CredentialsViewModelState.Default) await UpdateCredential();
                    else if (State == CredentialsViewModelState.Adding) await CreateCredential();
                }
            }
            finally
            {
                await UITools.HideSpinner();
            }
        }

        [RelayCommand]
        private async Task SelectedCredentialChanged(object parameter)
        {
            /* Pending changes not saved, ask for user confirmation before loosing changes */
            if (_requestConfirmation && !_confirmationInProcess && State == CredentialsViewModelState.Default)
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
                if(State == CredentialsViewModelState.Empty && SelectedCredential != null) State = CredentialsViewModelState.Default;
            }
        }

        [RelayCommand]
        private async Task DeleteCredential()
        {
            try
            {
                await UITools.ShowSpinner();
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

                State = CredentialsViewModelState.Empty;
            }
            finally
            {
                await UITools.HideSpinner();
            }
        }

        private async Task UpdateCredential()
        {
            Credential credential = Credentials.First(c => c.UUID == SelectedCredential.UUID);
            Credential credentialUpdated = SelectedCredential;
            if (credential.Password != SelectedCredential.Password)
            {
                (byte[], byte[]) encryptedValues = EncryptionTool.EncryptAes(SelectedCredential.Password, UITools.GetMasterKey(), UITools.GetMasterKeySalt());
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

            RequestFocusOnFirstField = true;
        }

        private async Task CreateCredential()
        {
            Credential newCredential = SelectedCredential;
            (byte[], byte[]) encryptedValues = EncryptionTool.EncryptAes(SelectedCredential.Password, UITools.GetMasterKey(), UITools.GetMasterKeySalt());
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

            RequestFocusOnFirstField = true;
        }

        private async Task ConfirmPendingChangesIfNeeded(Action action)
        {
            if (_requestConfirmation)
            {
                bool cancelNewSelection = await UITools.ShowContinueConfirmationDialogAsync("Important", "You have pending changes, do you want to continue?");
                if (cancelNewSelection) return;
            }

            action.Invoke();
        }
    }
}
