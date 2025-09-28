using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Desktiny.UI.Services;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    internal partial class ConfirmMasterKeyViewModel : ObservableObject
    {
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly IDialogManager _dialogManager;
        private readonly IProgressBarService _progressBarService;
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultLocalDb _shelterVaultLocalDb;
        private readonly IUIThreadService _uiThreadService;
        private readonly IWeakReferenceInstanceManager _weakReferenceInstanceManager;
        private readonly IVaultManager _vaultManager;

        [ObservableProperty]
        public partial List<ShelterVaultModel> Vaults { get; set; }
        [ObservableProperty]
        public partial ShelterVaultModel SelectedVault { get; set; }
        [ObservableProperty]
        public partial bool ShowPassword { get; set; }

        public ConfirmMasterKeyViewModel(IShelterVaultStateService shelterVaultStateService, IDialogManager dialogManager, IProgressBarService progressBarService, IShelterVault shelterVault, IUIThreadService uiThreadService, IWeakReferenceInstanceManager weakReferenceInstanceManager, IShelterVaultLocalDb shelterVaultLocalDb, IVaultManager shelterVaultCreatorManager)
        {
            _shelterVaultStateService = shelterVaultStateService;
            _dialogManager = dialogManager;
            _progressBarService = progressBarService;
            _shelterVault = shelterVault;
            _uiThreadService = uiThreadService;
            _shelterVaultLocalDb = shelterVaultLocalDb;
            _weakReferenceInstanceManager = weakReferenceInstanceManager;
            _vaultManager = shelterVaultCreatorManager;
            Vaults = shelterVault.GetAllActiveVaults().ToList();
            if (Vaults.Any()) SelectedVault = Vaults.FirstOrDefault();
            RegisterMessages();
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
                if (_vaultManager.IsValid(parameter?.ToString(), SelectedVault))
                {
                    _shelterVaultLocalDb.SetDbName(SelectedVault.Name);
                    _shelterVaultStateService.SetVault(SelectedVault, parameter?.ToString());
                    WeakReferenceMessenger.Default.Send(new CurrentAppStateRequestMessage(Shared.Enums.ShelterVaultAppState.NavigationView));
                }
                else await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_WRONG_MASTER_KEY);
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }

        private void RegisterMessages()
        {
            _weakReferenceInstanceManager.AddInstance(this);
            WeakReferenceMessenger.Default.Register<ConfirmMasterKeyViewModel, RefreshVaultListRequestMessage>(this, (viewModel, payload) =>
            {
                _uiThreadService.Execute(() =>
                {
                    if (payload.Value)
                    {
                        string selectedVaultTmp = viewModel.SelectedVault.UUID;
                        viewModel.Vaults = viewModel._shelterVault.GetAllActiveVaults().ToList();
                        if (viewModel.Vaults.Any())
                        {
                            viewModel.SelectedVault = null;
                            viewModel.SelectedVault = viewModel.Vaults.Find(x => x.UUID.Equals(selectedVaultTmp));
                        }
                    }
                });
            });
        }


        [RelayCommand]
        private void ChangePasswordVisibility()
        {
            ShowPassword = !ShowPassword;
        }
    }
}
