using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.Managers;
using ShelterVault.Services;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.ViewModels
{
    internal partial class CreateMasterKeyViewModel : ObservableObject
    {
        [ObservableProperty]
        private PasswordConfirmationViewModel _passwordRequirementsVM;
        [ObservableProperty]
        private string _name;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private string _passwordConfirmation;

        private readonly IShelterVaultCreatorManager _shelterVaultCreatorManager;
        private readonly IProgressBarService _progressBarService;

        public CreateMasterKeyViewModel(IShelterVaultCreatorManager shelterVaultCreatorManager, PasswordConfirmationViewModel passwordConfirmationViewModel, IProgressBarService progressBarService)
        {
            _shelterVaultCreatorManager = shelterVaultCreatorManager;
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
                    bool wasVaultCreated = _shelterVaultCreatorManager.CreateVault(Name, Password, Guid.NewGuid().ToString());
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
