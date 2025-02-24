using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        [ObservableProperty]
        private bool _showSync;
        [ObservableProperty]
        private bool _showLangOptions;
        [ObservableProperty]
        private string _englishLangOptionText;
        [ObservableProperty]
        private string _spanishLangOptionText;
        [ObservableProperty]
        private string _switchVaultText;
        [ObservableProperty]
        private CloudSyncStatus _currentCloudSyncStatus;

        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IShelterVaultThemeService _shelterVaultThemeService;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly ILanguageService _languageService;
        private readonly ICloudSyncManager _cloudSyncManager;
        private readonly IUIThreadService _uiThreadService;
        private readonly IWeakReferenceInstanceManager _weakReferenceInstanceManager;

        [RelayCommand]
        private void SwitchVault()
        {
            _shelterVaultStateService.ResetState();
            WeakReferenceMessenger.Default.Send(new CurrentAppStateRequestMessage(ShelterVaultAppState.ConfirmMasterKey));
        }

        [RelayCommand]
        private void ChangeTheme()
        {
            CurrentTheme = _shelterVaultThemeService.GetNextTheme(CurrentTheme);
        }

        [RelayCommand]
        private void Sync()
        {
            if(ShelterVaultCurrentAppState == ShelterVaultAppState.NavigationView)
            {
                WeakReferenceMessenger.Default.Send(new ShowPageRequestMessage(ShelterVaultPage.SETTINGS));
            }
        }

        public MainWindowViewModel(IShelterVaultLocalStorage shelterVaultLocalStorage, IShelterVaultThemeService shelterVaultThemeService, IShelterVaultStateService shelterVaultStateService, ILanguageService languageService, ICloudSyncManager cloudSyncManager, IUIThreadService uiThreadService, IWeakReferenceInstanceManager weakReferenceInstanceManager)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _shelterVaultThemeService = shelterVaultThemeService;
            _shelterVaultStateService = shelterVaultStateService;
            _languageService = languageService;
            _cloudSyncManager = cloudSyncManager;
            _uiThreadService = uiThreadService;
            _weakReferenceInstanceManager = weakReferenceInstanceManager;
            InitialSetup();
        }

        private void InitialSetup()
        {
            RegisterMessages();
            CurrentTheme = _shelterVaultThemeService.GetTheme();
            if (_shelterVaultLocalStorage.DBExists()) ShelterVaultCurrentAppState = ShelterVaultAppState.ConfirmMasterKey;
            ShowLangOptions = true;
            ShowSwitchVault = false;
            SetLangText();
            //StartSynchronizationTask();
        }

        private void RegisterMessages()
        {
            _weakReferenceInstanceManager.AddInstance(this);
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, CurrentAppStateRequestMessage>(this, (receiver, message) =>
            {
                receiver.ShelterVaultCurrentAppState = message.Value;
                if(message.Value == ShelterVaultAppState.NavigationView)
                {
                    ShowLangOptions = false;
                    ShowSwitchVault = true;
                }
                else
                {
                    ShowLangOptions = true;
                    ShowSwitchVault = false;
                }
            });
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, UpdateLanguageValuesMessage>(this, (receiver, message) =>
            {
                SetLangText();
            });
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, ProgressBarRequestMessage>(this, (receiver, message) =>
            {
                IsProgressBarVisible = message.Value;
            });
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, RefreshCurrentSyncStatusMessage>(this, (receiver, message) =>
            {
                _uiThreadService.Execute(() =>
                {
                    CurrentCloudSyncStatus = message.Value;
                });
            });
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, CloudProviderChangedMessage>(this, (receiver, message) =>
            {
                _uiThreadService.Execute(() =>
                {
                    CloudSyncInformation cloudSyncInformation = _cloudSyncManager.GetCurrentCloudSyncInformation();
                    ShowSync = cloudSyncInformation.HasCloudConfiguration;
                    CurrentCloudSyncStatus = cloudSyncInformation.CurrentSyncStatus;
                });
            });
        }

        private void SetLangText()
        {
            EnglishLangOptionText = _languageService.GetLangValue(LangResourceKeys.ENGLISH_OPTION);
            SpanishLangOptionText = _languageService.GetLangValue(LangResourceKeys.SPANISH_OPTION);
            SwitchVaultText = _languageService.GetLangValue(LangResourceKeys.SWITCH_VAULT);
            CloudSyncStatus tmpStatus = CurrentCloudSyncStatus;
            CurrentCloudSyncStatus = CloudSyncStatus.None;
            CurrentCloudSyncStatus = tmpStatus;
        }

        private void RefreshSyncStatusInfo()
        {
            CloudSyncInformation cloudSyncInformation = _cloudSyncManager.GetCurrentCloudSyncInformation();
            ShowSync = cloudSyncInformation.HasCloudConfiguration;
            CurrentCloudSyncStatus = cloudSyncInformation.CurrentSyncStatus;
        }

        private void StartSynchronizationTask()
        {
            int syncInterval = 60 * 1000;
            RefreshSyncStatusInfo();
            _ = Task.Run(async () =>
            {
                await Task.Delay(syncInterval);
                while (true)
                {
                    CloudSyncInformation cloudSyncInformation = _cloudSyncManager.GetCurrentCloudSyncInformation();
                    try
                    {
                        if (cloudSyncInformation.HasCloudConfiguration)
                            await _cloudSyncManager.SyncVaults();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    finally
                    {
                        if (cloudSyncInformation.HasCloudConfiguration)
                        {
                            _uiThreadService.Execute(() =>
                            {
                                RefreshSyncStatusInfo();
                            });
                        }
                    }

                    await Task.Delay(syncInterval);
                }
            });
        }
    }
}
