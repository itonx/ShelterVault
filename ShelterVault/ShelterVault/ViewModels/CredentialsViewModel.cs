using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Extensions;
using ShelterVault.Shared.Interfaces;
using ShelterVault.Shared.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    internal partial class CredentialsViewModel : ObservableObject, INavigation, IPendingChangesChallenge
    {
        private readonly IDialogService _dialogService;
        private readonly IProgressBarService _progressBarService;
        private readonly ICredentialsManager _credentialsManager;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly ILanguageService _languageService;
        private readonly IWeakReferenceInstanceManager _weakReferenceInstanceManager;

        private CancellationTokenSource _cancellationTokenSource;
        private Credentials _selectedCredentialBackup;

        [ObservableProperty]
        private Credentials _selectedCredential;
        [ObservableProperty]
        private bool _showPassword = false;
        [ObservableProperty]
        private CredentialsViewModelState _state = CredentialsViewModelState.New;
        [ObservableProperty]
        private bool _requestFocusOnFirstField;
        [ObservableProperty]
        private PasswordConfirmationViewModel _passwordRequirementsVM;

        public bool ChallengeCompleted { get; private set; } = false;

        public CredentialsViewModel(IDialogService dialogService, IProgressBarService progressBarService, PasswordConfirmationViewModel passwordConfirmationViewModel, ICredentialsManager credentialsManager, IShelterVaultStateService shelterVaultStateService, ILanguageService languageService, IUIThreadService uiThreadService, IWeakReferenceInstanceManager weakReferenceInstanceManager)
        {
            _dialogService = dialogService;
            _progressBarService = progressBarService;
            _credentialsManager = credentialsManager;
            _languageService = languageService;
            _shelterVaultStateService = shelterVaultStateService;
            _weakReferenceInstanceManager = weakReferenceInstanceManager;
            PasswordRequirementsVM = passwordConfirmationViewModel;
            PasswordRequirementsVM.HeaderText = _languageService.GetLangValue(LangResourceKeys.PASSWORD_MUST);
            RequestFocusOnFirstField = true;
            NewCredentials();
            RegisterMessages();
        }

        public void OnNavigated(object parameter)
        {
            CredentialsViewItem credentialParameter = ((CredentialsViewItem)parameter).Clone();
            SelectedCredential = _credentialsManager.GetCredentials(credentialParameter);
            _selectedCredentialBackup = SelectedCredential.Clone();
            State = CredentialsViewModelState.Updating;
        }

        public async Task<bool> DiscardChangesAsync(bool completeChallenge = false)
        {
            if (!_selectedCredentialBackup.Equals(SelectedCredential))
            {
                bool discard = await _dialogService.ShowContinueConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_PENDING_CHANGES, expectedResult: ContentDialogResult.Secondary);
                if (completeChallenge && discard) ChallengeCompleted = true;
                return discard;
            }
            else
            {
                if (completeChallenge) ChallengeCompleted = true;
                return true;
            }
        }

        [RelayCommand]
        private async Task CancelCredential()
        {
            if (!await DiscardChangesAsync(completeChallenge: true)) return;
            WeakReferenceMessenger.Default.Send(new ShowPageRequestMessage(Shared.Enums.ShelterVaultPage.HOME));
        }

        private void NewCredentials()
        {
            if (_shelterVaultStateService == null) throw new NullReferenceException("State hasn't been initialized.");

            ShowPassword = false;
            RequestFocusOnFirstField = true;
            State = CredentialsViewModelState.New;
            Credentials newCredential = new Credentials(_shelterVaultStateService.ShelterVault.UUID);
            newCredential.Password = newCredential.PasswordConfirmation = string.Empty;
            SelectedCredential = newCredential;
            _selectedCredentialBackup = newCredential.Clone();
        }

        [RelayCommand]
        private void SetClipboard()
        {
            if (_cancellationTokenSource != null)
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
        private void ChangePasswordVisibility() => ShowPassword = !ShowPassword;

        [RelayCommand]
        private async Task SaveCredentialChanges()
        {
            try
            {
                if (await PasswordRequirementsVM.AreCredentialsValid(SelectedCredential))
                {
                    await _progressBarService.Show();
                    if (State == CredentialsViewModelState.Updating) await UpdateCredential();
                    else if (State == CredentialsViewModelState.New) await CreateCredential();
                    WeakReferenceMessenger.Default.Send(new SelectCredentialRequestMessage(SelectedCredential));
                }
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }

        [RelayCommand]
        private async Task DeleteCredential()
        {
            try
            {
                if (!await DiscardChangesAsync(completeChallenge: true)) return;
                await _progressBarService.Show();
                if (SelectedCredential == null || string.IsNullOrWhiteSpace(SelectedCredential.UUID)) return;
                string uuid = SelectedCredential.UUID;
                if (await _credentialsManager.DeleteCredentials(uuid))
                {
                    WeakReferenceMessenger.Default.Send(new ShowPageRequestMessage(Shared.Enums.ShelterVaultPage.HOME));
                    WeakReferenceMessenger.Default.Send(new RefreshCredentialListRequestMessage(true));
                    await _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_DELETED);
                }
                else await _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_NOT_DELETED);
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }

        private async Task UpdateCredential()
        {
            Credentials credentials = await _credentialsManager.UpdateCredentials(SelectedCredential);
            if (credentials != null)
            {
                SelectedCredential = credentials;
                _selectedCredentialBackup = credentials.Clone();
                WeakReferenceMessenger.Default.Send(new RefreshCredentialListRequestMessage(true));
                await _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_UPDATED);
            }
            else await _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_NOT_UPDATED);

            RequestFocusOnFirstField = true;
        }

        private async Task CreateCredential()
        {
            Credentials credentials = await _credentialsManager.InsertCredentials(SelectedCredential);
            if (credentials != null)
            {
                SelectedCredential = credentials;
                _selectedCredentialBackup = SelectedCredential.Clone();
                State = CredentialsViewModelState.Updating;
                WeakReferenceMessenger.Default.Send(new RefreshCredentialListRequestMessage(true));
                await _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_SAVED);
            }
            else await _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_NOT_SAVED);

            RequestFocusOnFirstField = true;
        }

        public void RegisterMessages()
        {
            _weakReferenceInstanceManager.AddInstance(this);
            WeakReferenceMessenger.Default.Register<CredentialsViewModel, CheckSelectedCredentialsAfterSyncMessage>(this, (r, m) =>
            {
                if (State == CredentialsViewModelState.Updating)
                {
                    Credentials updatedCredentials = _credentialsManager.GetCredentials(SelectedCredential.UUID);
                    if (updatedCredentials == null)
                    {
                        _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_REMOVED_FROM_CLOUD);
                        WeakReferenceMessenger.Default.Send(new ShowPageRequestMessage(Shared.Enums.ShelterVaultPage.HOME));
                    }
                    else
                    {
                        if (SelectedCredential.Version != updatedCredentials.Version)
                        {
                            _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_UPDATED_IN_CLOUD);
                            SelectedCredential = updatedCredentials;
                            _selectedCredentialBackup = SelectedCredential.Clone();
                        }
                    }
                }
            });
        }
    }
}
