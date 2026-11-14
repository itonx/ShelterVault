using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Desktiny.WinUI.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using ShelterVault.DataLayer;
using ShelterVault.Interfaces;
using ShelterVault.Models;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Messages;
using Windows.ApplicationModel;

namespace ShelterVault.ViewModels
{
    internal partial class VaultMigrationViewModel : ObservableObject
    {
        private readonly IDialogManager _dialogManager;
        private readonly IProgressBarService _progressBarService;
        private readonly ILogger<SettingsViewModel> _logger;
        private readonly IEncryptionMigrationManager _encryptionMigrationManager;
        private readonly IVaultManager _vaultManager;
        private readonly ILanguageService _languageService;

        [ObservableProperty]
        public partial bool DisplayArgonMigration { get; set; } = false;

        [ObservableProperty]
        public partial string VaultMigrationMessage { get; set; }

        public VaultMigrationViewModel(
            IDialogManager dialogManager,
            IProgressBarService progressBarService,
            ILogger<SettingsViewModel> logger,
            IEncryptionMigrationManager encryptionMigrationManager,
            IVaultManager vaultManager,
            ILanguageService languageService
        )
        {
            _dialogManager = dialogManager;
            _progressBarService = progressBarService;
            _logger = logger;
            _vaultManager = vaultManager;
            _languageService = languageService;
            DisplayArgonMigration = encryptionMigrationManager.IsArgonMigrationAvailable();
            _encryptionMigrationManager = encryptionMigrationManager;
            var currentVault = _vaultManager.GetCurrentVault();
            VaultMigrationMessage =
                $"{_languageService.GetLangValue("Argon2MigrationDescription/Text")?.Replace("{VAULT_NAME}", currentVault.Name)}";
        }

        [RelayCommand]
        private async Task MigrateToArgon2()
        {
            try
            {
                var mk = await _dialogManager.ShowNewVaultMigrationDialog();

                if (string.IsNullOrWhiteSpace(mk))
                    return;

                await _progressBarService.Show();
                var currentVault = _vaultManager.GetCurrentVault();
                await _encryptionMigrationManager.MigrateEncryptedDataToArgon2Async(
                    currentVault.Version,
                    3,
                    mk
                );
                await Task.Delay(1500);
                await _dialogManager.ShowConfirmationDialogAsync(
                    LangResourceKeys.DIALOG_ARGON_MIGRATION_NOTIFICATION_OK
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on vault migration");
                await _dialogManager.ShowConfirmationDialogAsync(
                    LangResourceKeys.DIALOG_ARGON_MIGRATION_NOTIFICATION_ERROR
                );
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }
    }
}
