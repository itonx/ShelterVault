using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Services;
using ShelterVault.Shared.Messages;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        private readonly IVaultCreatorManager _vaultCreatorManager;
        private readonly IProgressBarService _progressBarService;

        public CreateMasterKeyViewModel(IVaultCreatorManager shelterVaultCreatorManager, PasswordConfirmationViewModel passwordConfirmationViewModel, IProgressBarService progressBarService, IShelterVault shelterVault, IShelterVaultLocalDb shelterVaultLocalDb)
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
                    string salt = Guid.NewGuid().ToString();
                    string uuid = Guid.NewGuid().ToString();
                    bool wasVaultCreated = _vaultCreatorManager.CreateVault(uuid, Name, Password, salt);
                    if (wasVaultCreated) WeakReferenceMessenger.Default.Send(new CurrentAppStateRequestMessage(Shared.Enums.ShelterVaultAppState.ConfirmMasterKey));
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
            WeakReferenceMessenger.Default.Send(new CurrentAppStateRequestMessage(Shared.Enums.ShelterVaultAppState.ConfirmMasterKey));
        }
    }
}
