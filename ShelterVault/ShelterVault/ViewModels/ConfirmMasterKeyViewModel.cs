using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Desktiny.WinUI.EventMessages;
using Desktiny.WinUI.Managers;
using Desktiny.WinUI.Services;
using ShelterVault.DataLayer;
using ShelterVault.Interfaces;
using ShelterVault.Models;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Messages;

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

        public ConfirmMasterKeyViewModel(
            IShelterVaultStateService shelterVaultStateService,
            IDialogManager dialogManager,
            IProgressBarService progressBarService,
            IShelterVault shelterVault,
            IUIThreadService uiThreadService,
            IWeakReferenceInstanceManager weakReferenceInstanceManager,
            IShelterVaultLocalDb shelterVaultLocalDb,
            IVaultManager shelterVaultCreatorManager
        )
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
            if (Vaults.Any())
                SelectedVault = Vaults.FirstOrDefault();
            RegisterMessages();
        }

        [RelayCommand]
        private void NewVault()
        {
            EventManager.Publish(new EnumNavigation(AppPage.CreateMasterKey));
        }

        [RelayCommand]
        private async Task ConfirmMasterKey(object parameter)
        {
            try
            {
                await _progressBarService.Show();
                byte[] encryptionKeyTmp;
                if (
                    _vaultManager.IsValid(
                        parameter?.ToString(),
                        SelectedVault,
                        out encryptionKeyTmp
                    )
                )
                {
                    _shelterVaultLocalDb.SetDbName(SelectedVault.Name);
                    if (SelectedVault.Version == (int)EncryptionVersion.v1)
                    {
                        _shelterVaultStateService.SetVault(SelectedVault, parameter?.ToString());
                    }
                    else if (SelectedVault.Version == (int)EncryptionVersion.v2)
                    {
                        _shelterVaultStateService.SetVault(SelectedVault, encryptionKeyTmp);
                    }
                    else
                    {
                        throw new ArgumentException("Version");
                    }

                    EventManager.Publish(new EnumNavigation(AppPage.NavigationView));
                }
                else
                    await _dialogManager.ShowConfirmationDialogAsync(
                        LangResourceKeys.DIALOG_WRONG_MASTER_KEY
                    );
            }
            catch (Exception ex)
            {
                await _dialogManager.ShowConfirmationDialogAsync(
                    LangResourceKeys.DIALOG_WRONG_MASTER_KEY
                );
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }

        private void RegisterMessages()
        {
            _weakReferenceInstanceManager.AddInstance(this);
            WeakReferenceMessenger.Default.Register<
                ConfirmMasterKeyViewModel,
                RefreshVaultListRequestMessage
            >(
                this,
                (viewModel, payload) =>
                {
                    _uiThreadService.Execute(() =>
                    {
                        if (payload.Value)
                        {
                            string selectedVaultTmp = viewModel.SelectedVault.UUID;
                            viewModel.Vaults = viewModel
                                ._shelterVault.GetAllActiveVaults()
                                .ToList();
                            if (viewModel.Vaults.Any())
                            {
                                viewModel.SelectedVault = null;
                                viewModel.SelectedVault = viewModel.Vaults.Find(x =>
                                    x.UUID.Equals(selectedVaultTmp)
                                );
                            }
                        }
                    });
                }
            );
        }

        [RelayCommand]
        private void ChangePasswordVisibility()
        {
            ShowPassword = !ShowPassword;
        }
    }
}
