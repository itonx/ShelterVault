using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Desktiny.WinUI.Services;
using ShelterVault.DataLayer;
using ShelterVault.Interfaces;
using ShelterVault.Models;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Messages;

namespace ShelterVault.ViewModels
{
    internal partial class NavigationViewModel : ObservableObject
    {
        private readonly ICredentialsManager _credentialsManager;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly IUIThreadService _uiThreadService;
        private readonly IShelterVault _shelterVault;
        private readonly IWeakReferenceInstanceManager _weakReferenceInstanceManager;

        [ObservableProperty]
        public partial IList<CredentialsViewItem> Credentials { get; set; }

        [ObservableProperty]
        public partial LoadPagePayload SelectedShelterVaultPage { get; set; }

        [ObservableProperty]
        public partial string VaultName { get; set; }

        [ObservableProperty]
        public partial bool IsPaneOpen { get; set; }

        public NavigationViewModel(
            ICredentialsManager credentialsManager,
            IShelterVaultStateService shelterVaultStateService,
            IUIThreadService uiThreadService,
            IShelterVault shelterVault,
            IWeakReferenceInstanceManager weakReferenceInstanceManager
        )
        {
            _credentialsManager = credentialsManager;
            _shelterVaultStateService = shelterVaultStateService;
            _uiThreadService = uiThreadService;
            _shelterVault = shelterVault;
            _weakReferenceInstanceManager = weakReferenceInstanceManager;
            Credentials = _credentialsManager
                .GetAllActiveCredentials(_shelterVaultStateService.ShelterVault.UUID)
                .ToList();
            SelectedShelterVaultPage = LoadPagePayload.Create(ShelterVaultPage.HOME);
            VaultName = _shelterVaultStateService.ShelterVault.Name;
            IsPaneOpen = true;
            RegisterMessages();
        }

        private void RegisterMessages()
        {
            _weakReferenceInstanceManager.AddInstance(this);
            WeakReferenceMessenger.Default.Register<NavigationViewModel, LoadCredentialsMessage>(
                this,
                (viewModel, payload) =>
                {
                    viewModel.SelectedShelterVaultPage = payload.Value;
                }
            );
            WeakReferenceMessenger.Default.Register<
                NavigationViewModel,
                RefreshCredentialListRequestMessage
            >(
                this,
                (viewModel, payload) =>
                {
                    _uiThreadService.Execute(() =>
                    {
                        if (payload.Value)
                        {
                            viewModel.Credentials = viewModel
                                ._credentialsManager.GetAllActiveCredentials(
                                    viewModel._shelterVaultStateService.ShelterVault.UUID
                                )
                                .ToList();
                        }
                    });
                }
            );
            WeakReferenceMessenger.Default.Register<
                NavigationViewModel,
                RefreshVaultListRequestMessage
            >(
                this,
                (viewModel, payload) =>
                {
                    _uiThreadService.Execute(() =>
                    {
                        ShelterVaultModel shelterVaultModel =
                            viewModel._shelterVault.GetVaultByUUID(
                                _shelterVaultStateService.ShelterVault.UUID
                            );
                        viewModel._shelterVaultStateService.SetVault(shelterVaultModel);
                        viewModel.VaultName = shelterVaultModel.Name;
                    });
                }
            );
            WeakReferenceMessenger.Default.Register<NavigationViewModel, IsPaneOpenMessage>(
                this,
                (viewModel, payload) =>
                {
                    viewModel.IsPaneOpen = !viewModel.IsPaneOpen;
                }
            );
        }
    }
}
