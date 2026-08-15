using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ShelterVault.Interfaces;
using ShelterVault.Shared.Constants;
using Windows.Storage;

namespace ShelterVault.ViewModels
{
    public class HomeViewModel : ObservableObject
    {
        private readonly IEncryptionMigrationManager _encryptionMigrationManager;
        private readonly IDialogManager _dialogManager;
        private readonly IShelterVaultStateService _shelterVaultStateService;

        public HomeViewModel(
            IEncryptionMigrationManager encryptionMigrationManager,
            IDialogManager dialogManager,
            IShelterVaultStateService shelterVaultStateService
        )
        {
            _encryptionMigrationManager = encryptionMigrationManager;
            _dialogManager = dialogManager;
            _shelterVaultStateService = shelterVaultStateService;
            CheckAvailableMigration();
        }

        private void CheckAvailableMigration()
        {
            string configKey =
                (_shelterVaultStateService?.ShelterVault?.Name ?? "") + "NextMigrationNotification";
            var localSettings = ApplicationData.Current.LocalSettings;
            var nextMigrationNotification =
                localSettings.Values[configKey]?.ToString() ?? DateTimeToString(DateTime.MinValue);
            DateTime nextMigrationNotificationDateTime = StringToDateTime(
                nextMigrationNotification
            );

            if (DateTime.UtcNow >= nextMigrationNotificationDateTime)
            {
                localSettings.Values[configKey] = DateTimeToString(DateTime.UtcNow.AddDays(3));

                if (_encryptionMigrationManager.IsArgonMigrationAvailable())
                {
                    _dialogManager.ShowConfirmationDialogAsync(
                        LangResourceKeys.DIALOG_ARGON_MIGRATION_NOTIFICATION
                    );
                }
            }
        }

        private string DateTimeToString(DateTime dateTime)
        {
            return dateTime.ToString("o", CultureInfo.InvariantCulture);
        }

        private DateTime StringToDateTime(string str)
        {
            return DateTime.Parse(str, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
    }
}
