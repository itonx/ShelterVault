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

        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IProgressBarService _progressBarService;

        public CreateMasterKeyViewModel(IShelterVaultLocalStorage shelterVaultLocalStorage, PasswordConfirmationViewModel passwordConfirmationViewModel, IProgressBarService progressBarService)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _progressBarService = progressBarService;
            PasswordRequirementsVM = passwordConfirmationViewModel;
        }

        [RelayCommand]
        private async Task CreateMasterKey(Dictionary<string, StringBuilder> masterKeyPasswords)
        {
            try
            {
                if (await PasswordRequirementsVM.ArePasswordsValid(masterKeyPasswords.Values.First().ToString(), masterKeyPasswords.Values.Last().ToString()))
                {
                    await _progressBarService.Show();
                    bool wasVaultCreated = _shelterVaultLocalStorage.CreateShelterVault(masterKeyPasswords.Values.First().ToString(), Guid.NewGuid().ToString());
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
