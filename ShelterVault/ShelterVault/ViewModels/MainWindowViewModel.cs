using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.DataLayer;
using ShelterVault.Services;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.ViewModels
{
    internal partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isProgressBarVisible = false;
        [ObservableProperty]
        private ShelterVaultAppState _shelterVaultCurrentAppState = ShelterVaultAppState.CreateMasterKey;
        [ObservableProperty]
        private ShelterVaultTheme _currentTheme;

        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IShelterVaultThemeService _shelterVaultThemeService;

        [RelayCommand]
        private void ChangeTheme()
        {
            CurrentTheme = _shelterVaultThemeService.GetNextTheme(CurrentTheme);
        }

        public MainWindowViewModel(IShelterVaultLocalStorage shelterVaultLocalStorage, IShelterVaultThemeService shelterVaultThemeService)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _shelterVaultThemeService = shelterVaultThemeService;
            InitialSetup();
        }

        private void InitialSetup()
        {
            RegisterMessages();
            CurrentTheme = _shelterVaultThemeService.GetTheme();
            if (_shelterVaultLocalStorage.DBExists()) ShelterVaultCurrentAppState = ShelterVaultAppState.ConfirmMasterKey;
        }

        private void RegisterMessages()
        {
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, CurrentAppStateRequestMessage>(this, (receiver, message) =>
            {
                receiver.ShelterVaultCurrentAppState = message.Value;
            });
        }
    }
}
