﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.DataLayer;
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
    public partial class CredentialsViewModel : ObservableObject, INavigation, IPendingChangesChallenge
    {
        private readonly IMasterKeyService _masterKeyService;
        private readonly IDialogService _dialogService;
        private readonly IProgressBarService _progressBarService;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IEncryptionService _encryptionService;

        private CancellationTokenSource _cancellationTokenSource;
        private Credential _selectedCredentialBackup;

        [ObservableProperty]
        private Credential _selectedCredential;
        [ObservableProperty]
        private bool _showPassword = false;
        [ObservableProperty]
        private CredentialsViewModelState _state = CredentialsViewModelState.New;
        [ObservableProperty]
        private bool _requestFocusOnFirstField;
        [ObservableProperty]
        private PasswordConfirmationViewModel _passwordRequirementsVM;

        public bool ChallengeCompleted { get; private set; } = false;

        public CredentialsViewModel(IMasterKeyService masterKeyService, IDialogService dialogService, IProgressBarService progressBarService, PasswordConfirmationViewModel passwordConfirmationViewModel, IShelterVaultLocalStorage shelterVaultLocalStorage, IEncryptionService encryptionService)
        {
            _masterKeyService = masterKeyService;
            _dialogService = dialogService;
            _progressBarService = progressBarService;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _encryptionService = encryptionService;
            PasswordRequirementsVM = passwordConfirmationViewModel;
            PasswordRequirementsVM.HeaderText = "Password must:";
            RequestFocusOnFirstField = true;
            NewCredentials();
        }

        public void OnNavigateTo(object parameter)
        {
            Credential credentialParameter = ((Credential)parameter);
            credentialParameter.Password = credentialParameter.PasswordConfirmation = _encryptionService.DecryptAes(Convert.FromBase64String(credentialParameter.EncryptedPassword), _masterKeyService.GetMasterKeyUnprotected(), Convert.FromBase64String(credentialParameter.InitializationVector), _masterKeyService.GetMasterKeySaltUnprotected());
            SelectedCredential = credentialParameter.Clone();
            _selectedCredentialBackup = credentialParameter.Clone();
            State = CredentialsViewModelState.Updating;
        }

        public async Task<bool> DiscardChangesAsync(bool completeChallenge = false)
        {
            if (!_selectedCredentialBackup.Equals(SelectedCredential))
            {
                bool discard = await _dialogService.ShowContinueConfirmationDialogAsync("Important", "You have pending changes, do you want to continue?", expectedResult: ContentDialogResult.Secondary);
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
            Credential newCredential = new Credential();
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
        private async Task DeleteCredential()
        {
            try
            {
                await _progressBarService.Show();
                if (SelectedCredential == null || string.IsNullOrWhiteSpace(SelectedCredential.UUID)) return;
                string uuid = SelectedCredential.UUID;
                if (_shelterVaultLocalStorage.DeleteCredential(uuid))
                {
                    WeakReferenceMessenger.Default.Send(new RefreshCredentialListRequestMessage(true));
                    WeakReferenceMessenger.Default.Send(new ShowPageRequestMessage(Shared.Enums.ShelterVaultPage.HOME));
                    await _dialogService.ShowConfirmationDialogAsync("Shelter Vault", "Credentials deleted.", "OK");
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
            Credential credentialUpdated = SelectedCredential;
            if (_selectedCredentialBackup.Password != SelectedCredential.Password)
            {
                (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(SelectedCredential.Password, _masterKeyService.GetMasterKeyUnprotected(), _masterKeyService.GetMasterKeySaltUnprotected());
                credentialUpdated = SelectedCredential.GenerateBase64EncryptedValues(encryptedValues);
            }

            bool updated = _shelterVaultLocalStorage.UpdateCredential(credentialUpdated);
            if (updated)
            {
                SelectedCredential = credentialUpdated;
                _selectedCredentialBackup = credentialUpdated.Clone();
                await _dialogService.ShowConfirmationDialogAsync("Important", "Chages were saved.");
            }
            else await _dialogService.ShowConfirmationDialogAsync("Important", "Your credential could't be updated.");

            RequestFocusOnFirstField = true;
        }

        private async Task CreateCredential()
        {
            (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(SelectedCredential.Password, _masterKeyService.GetMasterKeyUnprotected(), _masterKeyService.GetMasterKeySaltUnprotected());
            Credential newCredential = SelectedCredential.GenerateBase64EncryptedValues(encryptedValues);

            bool inserted = _shelterVaultLocalStorage.InsertCredential(newCredential);
            if (inserted)
            {
                SelectedCredential = newCredential;
                _selectedCredentialBackup = newCredential.Clone();
                await _dialogService.ShowConfirmationDialogAsync("Important", "Your credential was saved.");
                State = CredentialsViewModelState.Updating;
            }
            else await _dialogService.ShowConfirmationDialogAsync("Important", "Your credential could't be saved.");

            RequestFocusOnFirstField = true;
        }
    }
}
