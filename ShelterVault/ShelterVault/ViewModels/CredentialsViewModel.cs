using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Desktiny.UI.Interfaces;
using Desktiny.UI.Services;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Extensions;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    internal partial class CredentialsViewModel : ObservableObject, INavigation, IPendingChangesChallenge
    {
        private const string URL_SEPARATOR = "|";
        private readonly IDialogManager _dialogManager;
        private readonly IProgressBarService _progressBarService;
        private readonly ICredentialsManager _credentialsManager;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly ILanguageService _languageService;
        private readonly IWeakReferenceInstanceManager _weakReferenceInstanceManager;

        private CancellationTokenSource _cancellationTokenSource;
        private Credentials _selectedCredentialBackup;

        [ObservableProperty]
        public partial Credentials SelectedCredential { get; set; }
        [ObservableProperty]
        public partial bool ShowPassword { get; set; }
        [ObservableProperty]
        public partial CredentialsViewModelState State { get; set; }
        [ObservableProperty]
        public partial bool RequestFocusOnFirstField { get; set; }
        [ObservableProperty]
        public partial PasswordConfirmationViewModel PasswordRequirementsVM { get; set; }
        [ObservableProperty]
        public partial ObservableCollection<Uri> Links { get; set; }
        [ObservableProperty]
        public partial string TypedUrl { get; set; }

        public bool ChallengeCompleted { get; private set; }

        public CredentialsViewModel(IDialogManager dialogManager, IProgressBarService progressBarService, PasswordConfirmationViewModel passwordConfirmationViewModel, ICredentialsManager credentialsManager, IShelterVaultStateService shelterVaultStateService, ILanguageService languageService, IWeakReferenceInstanceManager weakReferenceInstanceManager)
        {
            _dialogManager = dialogManager;
            _progressBarService = progressBarService;
            _credentialsManager = credentialsManager;
            _languageService = languageService;
            _shelterVaultStateService = shelterVaultStateService;
            _weakReferenceInstanceManager = weakReferenceInstanceManager;
            PasswordRequirementsVM = passwordConfirmationViewModel;
            PasswordRequirementsVM.HeaderText = _languageService.GetLangValue(LangResourceKeys.PASSWORD_MUST);
            RequestFocusOnFirstField = true;
            State = CredentialsViewModelState.New;
            NewCredentials();
            RegisterMessages();
            Links = new ObservableCollection<Uri>();
        }

        [RelayCommand]
        private void UrlAdded()
        {
            IList<string> newUrlList = SelectedCredential.Url.Split(URL_SEPARATOR).ToList();
            newUrlList.Add(TypedUrl);
            SelectedCredential.Url = string.Join(URL_SEPARATOR, newUrlList);
            Links.Add(GetUrl(TypedUrl));
            TypedUrl = string.Empty;
        }

        [RelayCommand]
        private void DeleteUrl(object url)
        {
            IEnumerable<string> newUrlList = SelectedCredential.Url.Split(URL_SEPARATOR).Where(u => !u.Equals(url));
            SelectedCredential.Url = string.Join(URL_SEPARATOR, newUrlList);
            Links = new ObservableCollection<Uri>(newUrlList.Select(u => GetUrl(u)));
        }

        public void OnNavigated(object parameter)
        {
            CredentialsViewItem credentialParameter = ((CredentialsViewItem)parameter).Clone();
            SelectedCredential = _credentialsManager.GetCredentials(credentialParameter);
            _selectedCredentialBackup = SelectedCredential.Clone();
            State = CredentialsViewModelState.Updating;
            Links = new ObservableCollection<Uri>(SelectedCredential.Url.Split(URL_SEPARATOR).Select(u => GetUrl(u)));
        }

        private Uri GetUrl(string url)
        {
            return url.Contains("http://") || url.Contains("https://") ? new Uri(url) : new Uri(string.Concat("https://", url));
        }

        public async Task<bool> DiscardChangesAsync(bool completeChallenge = false)
        {
            if (!_selectedCredentialBackup.Equals(SelectedCredential))
            {
                bool discard = await _dialogManager.ShowContinueConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_PENDING_CHANGES, expectedResult: ContentDialogResult.Secondary);
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
        private void CopyID()
        {
            SelectedCredential.UUID.SendToClipboard();
        }

        [RelayCommand]
        private async Task CancelCredential()
        {
            if (!await DiscardChangesAsync(completeChallenge: true)) return;
            WeakReferenceMessenger.Default.Send(new ShowPageRequestMessage(Shared.Enums.ShelterVaultPage.HOME));
        }

        private void NewCredentials()
        {
            if (_shelterVaultStateService == null) throw new FieldAccessException("State hasn't been initialized.");

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
                    await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_DELETED);
                }
                else await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_NOT_DELETED);
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
                await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_UPDATED);
            }
            else await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_NOT_UPDATED);

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
                await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_SAVED);
            }
            else await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_NOT_SAVED);

            RequestFocusOnFirstField = true;
        }

        public void RegisterMessages()
        {
            _weakReferenceInstanceManager.AddInstance(this);
            WeakReferenceMessenger.Default.Register<CredentialsViewModel, CheckSelectedCredentialsAfterSyncMessage>(this, async (viewModel, payload) =>
            {
                if (viewModel.State == CredentialsViewModelState.Updating)
                {
                    Credentials updatedCredentials = viewModel._credentialsManager.GetCredentials(SelectedCredential.UUID);
                    if (updatedCredentials == null)
                    {
                        viewModel.ChallengeCompleted = true;
                        await viewModel._dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_REMOVED_FROM_CLOUD);
                        WeakReferenceMessenger.Default.Send(new ShowPageRequestMessage(Shared.Enums.ShelterVaultPage.HOME));
                    }
                    else
                    {
                        if (viewModel.SelectedCredential.Version != updatedCredentials.Version)
                        {
                            await viewModel._dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_UPDATED_IN_CLOUD);
                            viewModel.SelectedCredential = updatedCredentials;
                            viewModel._selectedCredentialBackup = viewModel.SelectedCredential.Clone();
                        }
                    }
                }
            });
        }
    }
}
