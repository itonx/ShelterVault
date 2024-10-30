using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    public partial class NavigationViewModel : ObservableObject
    {
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;

        [ObservableProperty]
        private IList<Credential> _credentials;
        [ObservableProperty]
        private bool _isHomeSelected = false;

        public NavigationViewModel(IShelterVaultLocalStorage shelterVaultLocalStorage)
        {
            RegisterMessages();
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            Credentials = (IList<Credential>)_shelterVaultLocalStorage.GetAllCredentials();
        }

        private void RegisterMessages()
        {
            WeakReferenceMessenger.Default.Register<NavigationViewModel, ShowPageRequestMessage>(this, (receiver, message) =>
            {
                if (message.Value == Shared.Enums.ShelterVaultPage.HOME) receiver.IsHomeSelected = true;
            });
        }
    }
}
