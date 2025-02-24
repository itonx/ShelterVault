using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Services;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        private readonly IShelterVaultCreatorManager _shelterVaultCreatorManager;
        private readonly IProgressBarService _progressBarService;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;

        public CreateMasterKeyViewModel(IShelterVaultCreatorManager shelterVaultCreatorManager, PasswordConfirmationViewModel passwordConfirmationViewModel, IProgressBarService progressBarService, IShelterVaultLocalStorage shelterVaultLocalStorage)
        {
            _shelterVaultCreatorManager = shelterVaultCreatorManager;
            _progressBarService = progressBarService;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            PasswordRequirementsVM = passwordConfirmationViewModel;
            ShowCancel = _shelterVaultLocalStorage.GetAllActiveVaults().Any();
            _shelterVaultDefaultPath = _shelterVaultLocalStorage.GetDefaultShelterVaultDBPath();
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
                    bool wasVaultCreated = _shelterVaultCreatorManager.CreateVault(uuid, Name, Password, salt);
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
