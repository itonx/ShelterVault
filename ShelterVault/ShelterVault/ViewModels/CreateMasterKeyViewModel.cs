using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.DataLayer;
using ShelterVault.Services;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.ViewModels
{
    public partial class CreateMasterKeyViewModel : ObservableObject
    {
        [ObservableProperty]
        private PasswordConfirmationViewModel _passwordRequirementsVM;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private string _passwordConfirmation;

        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IProgressBarService _progressBarService;

        public CreateMasterKeyViewModel(IShelterVaultLocalStorage shelterVaultLocalStorage, PasswordConfirmationViewModel passwordConfirmationViewModel, IProgressBarService progressBarService)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _progressBarService = progressBarService;
            PasswordRequirementsVM = passwordConfirmationViewModel;
        }

        [RelayCommand]
        private async Task CreateMasterKey()
        {
            try
            {
                if (await PasswordRequirementsVM.ArePasswordsValid(Password, PasswordConfirmation))
                {
                    await _progressBarService.Show();
                    bool wasVaultCreated = _shelterVaultLocalStorage.CreateShelterVault(Password, Guid.NewGuid().ToString());
                    if (wasVaultCreated) WeakReferenceMessenger.Default.Send(new CurrentAppStateRequestMessage(Shared.Enums.ShelterVaultAppState.ConfirmMasterKey));
                }
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }
    }
}
