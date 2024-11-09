using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;
using ShelterVault.Shared.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.ViewModels
{
    internal partial class ConfirmMasterKeyViewModel : ObservableObject
    {
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly IDialogService _dialogService;
        private readonly IProgressBarService _progressBarService;
        private readonly IMasterKeyValidatorManager _masterKeyValidatorManager;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
 
        [ObservableProperty]
        private List<ShelterVaultModel> _vaults;
        [ObservableProperty]
        private ShelterVaultModel _selectedVault;

        public ConfirmMasterKeyViewModel(IShelterVaultStateService shelterVaultStateService, IDialogService dialogService, IProgressBarService progressBarService, IMasterKeyValidatorManager masterKeyValidatorManager, IShelterVaultLocalStorage shelterVaultLocalStorage)
        {
            _shelterVaultStateService = shelterVaultStateService;
            _dialogService = dialogService;
            _progressBarService = progressBarService;
            _masterKeyValidatorManager = masterKeyValidatorManager;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            Vaults = _shelterVaultLocalStorage.GetAllVaults().ToList();
            if (Vaults.Any()) SelectedVault = Vaults.FirstOrDefault();
        }

        [RelayCommand]
        private void NewVault()
        {
            WeakReferenceMessenger.Default.Send(new CurrentAppStateRequestMessage(Shared.Enums.ShelterVaultAppState.CreateMasterKey));
        }


        [RelayCommand]
        private async Task ConfirmMasterKey(object parameter)
        {
            try
            {
                await _progressBarService.Show();
                if (_masterKeyValidatorManager.IsValid(parameter?.ToString(), SelectedVault))
                {
                    _shelterVaultStateService.SetVault(SelectedVault, parameter?.ToString());
                    WeakReferenceMessenger.Default.Send(new CurrentAppStateRequestMessage(Shared.Enums.ShelterVaultAppState.NavigationView));
                }
                else await _dialogService.ShowConfirmationDialogAsync("Important", "Wrong master key!");
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }
    }
}
