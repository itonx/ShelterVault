using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;
using ShelterVault.Shared.Messages;
using ShelterVault.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
 
namespace ShelterVault.ViewModels
{
    public partial class CredentialsViewModel : ObservableObject, INavigation
    {
        private readonly IMasterKeyService _masterKeyService;
        private readonly IDialogService _dialogService;
        private readonly IProgressBarService _progressBarService;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IEncryptionService _encryptionService;

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
                    value.Password = _encryptionService.DecryptAes(Convert.FromBase64String(value.EncryptedPassword), _masterKeyService.GetMasterKeyUnprotected(), Convert.FromBase64String(value.InitializationVector), _masterKeyService.GetMasterKeySaltUnprotected());
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
        private PasswordConfirmationViewModel _passwordRequirementsVM;

        public CredentialsViewModel(IMasterKeyService masterKeyService, IDialogService dialogService, IProgressBarService progressBarService, PasswordConfirmationViewModel passwordConfirmationViewModel, IShelterVaultLocalStorage shelterVaultLocalStorage, IEncryptionService encryptionService)
        {
            _masterKeyService = masterKeyService;
            _dialogService = dialogService;
            _progressBarService = progressBarService;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _encryptionService = encryptionService;
            PasswordRequirementsVM = passwordConfirmationViewModel;
            Credentials = new ObservableCollection<Credential>(_shelterVaultLocalStorage.GetAllCredentials());
            PasswordRequirementsVM.HeaderText = "Password must:";
            RequestFocusOnFirstField = true;
            NewCredentialInternal();
        }

        public void OnNavigateTo(object parameter)
        {
            SelectedCredential = ((Credential)parameter).Clone();
            State = CredentialsViewModelState.Default;
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
            WeakReferenceMessenger.Default.Send(new ShowPageRequestMessage(Shared.Enums.ShelterVaultPage.HOME));
        }

        [RelayCommand]
        private async Task NewCredential()
        {
            await ConfirmPendingChangesIfNeeded(() =>
            {
                NewCredentialInternal();
            });
        }

        private void NewCredentialInternal()
        {
            ShowPassword = false;
            RequestFocusOnFirstField = true;
            State = CredentialsViewModelState.Adding;
            Credential newCredential = new Credential();
            newCredential.Password = newCredential.PasswordConfirmation = string.Empty;
            SelectedCredential = null;
            SelectedCredential = newCredential;
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
                    await _progressBarService.Show();
                    if (State == CredentialsViewModelState.Default) await UpdateCredential();
                    else if (State == CredentialsViewModelState.Adding) await CreateCredential();
                    WeakReferenceMessenger.Default.Send(new RefreshCredentialListRequestMessage(true));
                    WeakReferenceMessenger.Default.Send(new SelectCredentialRequestMessage(SelectedCredential));
                }
            }
            finally
            {
                await _progressBarService.Hide();
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
                bool cancelNewSelection = await _dialogService.ShowContinueConfirmationDialogAsync("Important", "You have pending changes, do you want to continue?");
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
                await _progressBarService.Show();
                if (SelectedCredential == null || string.IsNullOrWhiteSpace(SelectedCredential.UUID)) return;
                string uuid = SelectedCredential.UUID;
                if (_shelterVaultLocalStorage.DeleteCredential(uuid))
                {
                    SelectedCredential = null;
                    SelectedCredential = new Credential();
                    SelectedCredential.Password = SelectedCredential.PasswordConfirmation = string.Empty;
                    Credential credential = Credentials.First(c => c.UUID == uuid);
                    Credentials.Remove(credential);
                    await _dialogService.ShowConfirmationDialogAsync("Shelter Vault", "Your credential was deleted.", "OK");
                }
                else await _dialogService.ShowConfirmationDialogAsync("Shelter Vault", "Your credential couldn't be deleted.", "OK");

                State = CredentialsViewModelState.Empty;
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }

        private async Task UpdateCredential()
        {
            Credential credential = Credentials.First(c => c.UUID == SelectedCredential.UUID);
            Credential credentialUpdated = SelectedCredential;
            if (credential.Password != SelectedCredential.Password)
            {
                (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(SelectedCredential.Password, _masterKeyService.GetMasterKeyUnprotected(), _masterKeyService.GetMasterKeySaltUnprotected());
                credentialUpdated = SelectedCredential.GetUpdatedCredentialValues(encryptedValues);
            }

            bool updated = _shelterVaultLocalStorage.UpdateCredential(credentialUpdated);
            if (updated)
            {
                Credential credentialUpdatedInView = Credentials.First(c => c.UUID == credentialUpdated.UUID);
                Credentials[Credentials.IndexOf(credentialUpdatedInView)] = credentialUpdated;
                SelectedCredential = null;
                SelectedCredential = Credentials.First(c => c.UUID == credentialUpdated.UUID);
                await _dialogService.ShowConfirmationDialogAsync("Important", "Chages were saved.");
            }
            else await _dialogService.ShowConfirmationDialogAsync("Important", "Your credential could't be updated.");

            RequestFocusOnFirstField = true;
        }

        private async Task CreateCredential()
        {
            Credential newCredential = SelectedCredential;
            (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(SelectedCredential.Password, _masterKeyService.GetMasterKeyUnprotected(), _masterKeyService.GetMasterKeySaltUnprotected());
            newCredential = SelectedCredential.GetUpdatedCredentialValues(encryptedValues);

            bool inserted = _shelterVaultLocalStorage.InsertCredential(newCredential);
            if (inserted)
            {
                Credentials.Add(newCredential);
                SelectedCredential = null;
                SelectedCredential = Credentials.First(c => c.UUID == newCredential.UUID);
                await _dialogService.ShowConfirmationDialogAsync("Important", "Your credential was saved.");
                State = CredentialsViewModelState.Default;
            }
            else await _dialogService.ShowConfirmationDialogAsync("Important", "Your credential could't be saved.");

            RequestFocusOnFirstField = true;
        }

        private async Task ConfirmPendingChangesIfNeeded(Action action)
        {
            if (_requestConfirmation)
            {
                bool cancelNewSelection = await _dialogService.ShowContinueConfirmationDialogAsync("Important", "You have pending changes, do you want to continue?");
                if (cancelNewSelection) return;
            }

            action.Invoke();
        }
    }
}
