using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;
using ShelterVault.Shared.Interfaces;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks; 

namespace ShelterVault.ViewModels
{
    internal partial class CredentialsViewModel : ObservableObject, INavigation, IPendingChangesChallenge
    {
        private readonly IDialogService _dialogService;
        private readonly IProgressBarService _progressBarService;
        private readonly ICredentialsManager _credentialsManager;

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

        public CredentialsViewModel(IDialogService dialogService, IProgressBarService progressBarService, PasswordConfirmationViewModel passwordConfirmationViewModel, ICredentialsManager credentialsManager)
        {
            _dialogService = dialogService;
            _progressBarService = progressBarService;
            _credentialsManager = credentialsManager;
            PasswordRequirementsVM = passwordConfirmationViewModel;
            PasswordRequirementsVM.HeaderText = "Password must:";
            RequestFocusOnFirstField = true;
            NewCredentials();
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
                bool discard = await _dialogService.ShowContinueConfirmationDialogAsync("Important", "You have pending changes, do you want to continue without saving changes?", expectedResult: ContentDialogResult.Secondary);
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
            ShowPassword = false;
            RequestFocusOnFirstField = true;
            State = CredentialsViewModelState.New;
            Credentials newCredential = new Credentials();
            newCredential.Password = newCredential.PasswordConfirmation = string.Empty;
            SelectedCredential = newCredential;
            _selectedCredentialBackup = newCredential.Clone();
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
                if (_credentialsManager.DeleteCredentials(uuid))
                {
                    WeakReferenceMessenger.Default.Send(new ShowPageRequestMessage(Shared.Enums.ShelterVaultPage.HOME));
                    WeakReferenceMessenger.Default.Send(new RefreshCredentialListRequestMessage(true));
                    await _dialogService.ShowConfirmationDialogAsync("Shelter Vault", "Your credentials were deleted.", "OK");
                }
                else await _dialogService.ShowConfirmationDialogAsync("Shelter Vault", "Your credentials couldn't be deleted.", "OK");
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }

        private async Task UpdateCredential()
        {
            Credentials credentials = _credentialsManager.UpdateCredentials(SelectedCredential);
            if (credentials != null)
            {
                SelectedCredential = credentials;
                _selectedCredentialBackup = credentials.Clone();
                WeakReferenceMessenger.Default.Send(new RefreshCredentialListRequestMessage(true));
                await _dialogService.ShowConfirmationDialogAsync("Important", "Your credentials were updated.");
            }
            else await _dialogService.ShowConfirmationDialogAsync("Important", "Your credentials could't be updated.");

            RequestFocusOnFirstField = true;
        }

        private async Task CreateCredential()
        {
            Credentials credentials = _credentialsManager.InsertCredentials(SelectedCredential);
            if (credentials != null)
            {
                SelectedCredential = credentials;
                _selectedCredentialBackup = SelectedCredential.Clone();
                State = CredentialsViewModelState.Updating;
                WeakReferenceMessenger.Default.Send(new RefreshCredentialListRequestMessage(true));
                await _dialogService.ShowConfirmationDialogAsync("Important", "Your credentials were saved.");
            }
            else await _dialogService.ShowConfirmationDialogAsync("Important", "Your credentials could't be saved.");

            RequestFocusOnFirstField = true;
        }
    }
}
