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
        [ObservableProperty]
        private bool _showSwitchVault;

        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IShelterVaultThemeService _shelterVaultThemeService;
        private readonly IShelterVaultStateService _shelterVaultStateService;

        [RelayCommand]
        private void SwitchVault()
        {
            _shelterVaultStateService.ResetState();
            WeakReferenceMessenger.Default.Send(new CurrentAppStateRequestMessage(Shared.Enums.ShelterVaultAppState.ConfirmMasterKey));
        }

        [RelayCommand]
        private void ChangeTheme()
        {
            CurrentTheme = _shelterVaultThemeService.GetNextTheme(CurrentTheme);
        }

        public MainWindowViewModel(IShelterVaultLocalStorage shelterVaultLocalStorage, IShelterVaultThemeService shelterVaultThemeService, IShelterVaultStateService shelterVaultStateService)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _shelterVaultThemeService = shelterVaultThemeService;
            _shelterVaultStateService = shelterVaultStateService;
            InitialSetup();
        }

        private void InitialSetup()
        {
            RegisterMessages();
            CurrentTheme = _shelterVaultThemeService.GetTheme();
            if (_shelterVaultLocalStorage.DBExists()) ShelterVaultCurrentAppState = ShelterVaultAppState.ConfirmMasterKey;
            ShowSwitchVault = false;
        }

        private void RegisterMessages()
        {
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, CurrentAppStateRequestMessage>(this, (receiver, message) =>
            {
                receiver.ShelterVaultCurrentAppState = message.Value;
                if(message.Value == ShelterVaultAppState.NavigationView)
                {
                    ShowSwitchVault = true;
                }
                else
                {
                    ShowSwitchVault = false;
                }
            });
        }
    }
}
