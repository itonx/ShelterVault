using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Desktiny.WinUI.EventMessages;
using Desktiny.WinUI.Managers;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using ShelterVault.DataLayer;
using ShelterVault.Interfaces;
using ShelterVault.Shared.Enums;

namespace ShelterVault.ViewModels
{
    internal partial class CreateMasterKeyViewModel : ObservableObject
    {
        private string _shelterVaultDefaultPath;

        [ObservableProperty]
        public partial PasswordConfirmationViewModel PasswordRequirementsVM { get; set; }

        [ObservableProperty]
        public partial string Name { get; set; }

        [ObservableProperty]
        public partial string Password { get; set; }

        [ObservableProperty]
        public partial string PasswordConfirmation { get; set; }

        [ObservableProperty]
        public partial string ShelterVaultPath { get; set; }

        [ObservableProperty]
        public partial bool ShowCancel { get; set; }

        [ObservableProperty]
        public partial string DefaultPath { get; set; }

        [ObservableProperty]
        public partial bool ShowPassword { get; set; }

        [ObservableProperty]
        public partial bool IsDialogMode { get; set; } = false;

        public string DialogResult { get; internal set; } = "";

        private readonly IVaultCreatorManager _vaultCreatorManager;
        private readonly IProgressBarService _progressBarService;

        public CreateMasterKeyViewModel(
            IVaultCreatorManager shelterVaultCreatorManager,
            PasswordConfirmationViewModel passwordConfirmationViewModel,
            IProgressBarService progressBarService,
            IShelterVault shelterVault,
            IShelterVaultLocalDb shelterVaultLocalDb
        )
        {
            _vaultCreatorManager = shelterVaultCreatorManager;
            _progressBarService = progressBarService;
            _shelterVaultDefaultPath = shelterVaultLocalDb.DefaultShelterVaultPath;
            PasswordRequirementsVM = passwordConfirmationViewModel;
            ShowCancel = shelterVault.GetAllActiveVaults().Any();
            DefaultPath = _shelterVaultDefaultPath;
        }

        partial void OnNameChanged(string value)
        {
            DefaultPath = Path.Combine(_shelterVaultDefaultPath, string.Concat(value, ".db"));
        }

        [RelayCommand]
        private async Task CreateMasterKey()
        {
            try
            {
                if (await PasswordRequirementsVM.ArePasswordsValid(Password, PasswordConfirmation))
                {
                    await _progressBarService.Show();
                    string uuid = Guid.NewGuid().ToString();
                    bool wasVaultCreated = _vaultCreatorManager.CreateVault(uuid, Name, Password);
                    if (wasVaultCreated)
                        EventManager.Publish(new EnumNavigation(AppPage.ConfirmMasterKey));
                }
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            EventManager.Publish(new EnumNavigation(AppPage.ConfirmMasterKey));
        }

        [RelayCommand]
        private void ChangePasswordVisibility()
        {
            ShowPassword = !ShowPassword;
        }
    }
}
