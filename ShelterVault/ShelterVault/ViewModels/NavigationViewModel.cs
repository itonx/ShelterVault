using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Desktiny.UI.Services;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Messages;
using System.Collections.Generic;
using System.Linq;

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
        private IList<CredentialsViewItem> _credentials;
        [ObservableProperty]
        private object _selectedMenuItem;
        [ObservableProperty]
        private string _vaultName;

        public NavigationViewModel(ICredentialsManager credentialsManager, IShelterVaultStateService shelterVaultStateService, IUIThreadService uiThreadService, IShelterVault shelterVault, IWeakReferenceInstanceManager weakReferenceInstanceManager)
        {
            _credentialsManager = credentialsManager;
            _shelterVaultStateService = shelterVaultStateService;
            _uiThreadService = uiThreadService;
            _shelterVault = shelterVault;
            _weakReferenceInstanceManager = weakReferenceInstanceManager;
            Credentials = _credentialsManager.GetAllActiveCredentials(_shelterVaultStateService.ShelterVault.UUID).ToList();
            SelectedMenuItem = Shared.Enums.ShelterVaultPage.HOME.ToString();
            VaultName = _shelterVaultStateService.ShelterVault.Name;
            RegisterMessages();
        }

        private void RegisterMessages()
        {
            _weakReferenceInstanceManager.AddInstance(this);
            WeakReferenceMessenger.Default.Register<NavigationViewModel, ShowPageRequestMessage>(this, (viewModel, payload) =>
            {
                if (payload.Value == Shared.Enums.ShelterVaultPage.HOME || payload.Value == Shared.Enums.ShelterVaultPage.SETTINGS)
                    viewModel.SelectedMenuItem = payload.Value.ToString();
            });
            WeakReferenceMessenger.Default.Register<NavigationViewModel, RefreshCredentialListRequestMessage>(this, (viewModel, payload) =>
            {
                _uiThreadService.Execute(() =>
                {
                    if (payload.Value)
                    {
                        viewModel.Credentials = viewModel._credentialsManager.GetAllActiveCredentials(viewModel._shelterVaultStateService.ShelterVault.UUID).ToList();
                        if (viewModel.SelectedMenuItem is CredentialsViewItem item)
                        {
                            CredentialsViewItem newSelectedItem = viewModel.Credentials.FirstOrDefault(x => x.UUID.Equals(item.UUID));
                            if (newSelectedItem != null)
                            {
                                newSelectedItem.SkipPageLoader = true;
                                viewModel.SelectedMenuItem = null;
                                viewModel.SelectedMenuItem = newSelectedItem;
                                newSelectedItem.SkipPageLoader = false;
                            }
                            WeakReferenceMessenger.Default.Send(new CheckSelectedCredentialsAfterSyncMessage(true));
                        }
                    }
                });
            });
            WeakReferenceMessenger.Default.Register<NavigationViewModel, SelectCredentialRequestMessage>(this, (viewModel, payload) =>
            {
                if (payload.Value != null)
                {
                    CredentialsViewItem selectTarget = viewModel.Credentials.FirstOrDefault(c => c.UUID.Equals(payload.Value.UUID));
                    viewModel.SelectedMenuItem = selectTarget;
                }
            });
            WeakReferenceMessenger.Default.Register<NavigationViewModel, RefreshVaultListRequestMessage>(this, (viewModel, payload) =>
            {
                _uiThreadService.Execute(() =>
                {
                    ShelterVaultModel shelterVaultModel = viewModel._shelterVault.GetVaultByUUID(_shelterVaultStateService.ShelterVault.UUID);
                    viewModel._shelterVaultStateService.SetVault(shelterVaultModel);
                    viewModel.VaultName = shelterVaultModel.Name;
                });
            });
        }
    }
}
