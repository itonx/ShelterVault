using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace ShelterVault.ViewModels
{
    internal partial class NavigationViewModel : ObservableObject
    {
        private readonly ICredentialsReaderManager _credentialsReaderManager;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly IUIThreadService _uiThreadService;
        private readonly IVaultReaderManager _vaultReaderManager;
        private readonly IWeakReferenceInstanceManager _weakReferenceInstanceManager;

        [ObservableProperty]
        private IList<CredentialsViewItem> _credentials;
        [ObservableProperty]
        private object _selectedMenuItem;
        [ObservableProperty]
        private string _vaultName;

        public NavigationViewModel(ICredentialsReaderManager credentialsReaderManager, IShelterVaultStateService shelterVaultStateService, IUIThreadService uiThreadService, IVaultReaderManager vaultReaderManager, IWeakReferenceInstanceManager weakReferenceInstanceManager)
        {
            _credentialsReaderManager = credentialsReaderManager;
            _shelterVaultStateService = shelterVaultStateService;
            _uiThreadService = uiThreadService;
            _vaultReaderManager = vaultReaderManager;
            _weakReferenceInstanceManager = weakReferenceInstanceManager;
            Credentials = _credentialsReaderManager.GetAllActiveCredentials(_shelterVaultStateService.ShelterVault.UUID).ToList();
            SelectedMenuItem = Shared.Enums.ShelterVaultPage.HOME.ToString();
            VaultName = _shelterVaultStateService.ShelterVault.Name;
            RegisterMessages();
        }

        private void RegisterMessages()
        {
            _weakReferenceInstanceManager.AddInstance(this);
            WeakReferenceMessenger.Default.Register<NavigationViewModel, ShowPageRequestMessage>(this, (receiver, message) =>
            {
                if (message.Value == Shared.Enums.ShelterVaultPage.HOME || message.Value == Shared.Enums.ShelterVaultPage.SETTINGS) 
                    receiver.SelectedMenuItem = message.Value.ToString();
            });
            WeakReferenceMessenger.Default.Register<NavigationViewModel, RefreshCredentialListRequestMessage>(this, (receiver, message) =>
            {
                _uiThreadService.Execute(() =>
                {
                    if (message.Value)
                    {
                        receiver.Credentials = receiver._credentialsReaderManager.GetAllActiveCredentials(receiver._shelterVaultStateService.ShelterVault.UUID).ToList();
                        if (receiver.SelectedMenuItem is CredentialsViewItem item)
                        {
                            CredentialsViewItem newSelectedItem = receiver.Credentials.FirstOrDefault(x => x.UUID.Equals(item.UUID));
                            if(newSelectedItem != null)
                            {
                                newSelectedItem.SkipPageLoader = true;
                                receiver.SelectedMenuItem = null;
                                receiver.SelectedMenuItem = newSelectedItem;
                                newSelectedItem.SkipPageLoader = false;
                            }
                            WeakReferenceMessenger.Default.Send(new CheckSelectedCredentialsAfterSyncMessage(true));
                        }
                    }
                });
            });
            WeakReferenceMessenger.Default.Register<NavigationViewModel, SelectCredentialRequestMessage>(this, (receiver, message) =>
            {
                if (message.Value != null)
                {
                    CredentialsViewItem selectTarget = Credentials.FirstOrDefault(c => c.UUID.Equals(message.Value.UUID));
                    receiver.SelectedMenuItem = selectTarget;
                }
            });
            WeakReferenceMessenger.Default.Register<NavigationViewModel, RefreshVaultListRequestMessage>(this, (receiver, message) =>
            {
                _uiThreadService.Execute(() => {
                    ShelterVaultModel shelterVaultModel = _vaultReaderManager.GetVaultById(_shelterVaultStateService.ShelterVault.UUID);
                    _shelterVaultStateService.SetVault(shelterVaultModel);
                    VaultName = shelterVaultModel.Name;
                });
            });
        }
    }
}
