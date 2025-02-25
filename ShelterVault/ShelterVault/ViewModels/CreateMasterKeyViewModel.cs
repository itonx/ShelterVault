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
        private PasswordConfirmationViewModel _passwordRequirementsVM;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private string _passwordConfirmation;
        [ObservableProperty]
        private string _shelterVaultPath;
        [ObservableProperty]
        private bool _showCancel;
        [ObservableProperty]
        private string _defaultPath;

        private readonly IVaultManager _vaultManager;
        private readonly IProgressBarService _progressBarService;

        public CreateMasterKeyViewModel(IVaultManager shelterVaultCreatorManager, PasswordConfirmationViewModel passwordConfirmationViewModel, IProgressBarService progressBarService, IShelterVault shelterVault, IShelterVaultLocalDb shelterVaultLocalDb)
        {
            _vaultManager = shelterVaultCreatorManager;
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
                    bool wasVaultCreated = _vaultManager.CreateVault(uuid, Name, Password, salt);
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
