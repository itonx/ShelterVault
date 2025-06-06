﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Desktiny.UI.Models;
using Desktiny.UI.Services;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Messages;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    internal partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial bool IsProgressBarVisible { get; set; }
        [ObservableProperty]
        public partial ShelterVaultAppState ShelterVaultCurrentAppState { get; set; }
        [ObservableProperty]
        public partial AppThemeModel CurrentAppTheme { get; set; }
        [ObservableProperty]
        public partial bool ShowSwitchVault { get; set; }
        [ObservableProperty]
        public partial bool ShowSync { get; set; }
        [ObservableProperty]
        public partial bool ShowLangOptions { get; set; }
        [ObservableProperty]
        public partial string EnglishLangOptionText { get; set; }
        [ObservableProperty]
        public partial string SpanishLangOptionText { get; set; }
        [ObservableProperty]
        public partial string SwitchVaultText { get; set; }
        [ObservableProperty]
        public partial CloudSyncStatus CurrentCloudSyncStatus { get; set; }

        private readonly IShelterVaultThemeService _shelterVaultThemeService;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly ILanguageService _languageService;
        private readonly ICloudSyncManager _cloudSyncManager;
        private readonly IUIThreadService _uiThreadService;
        private readonly IWeakReferenceInstanceManager _weakReferenceInstanceManager;

        public MainWindowViewModel(IShelterVault shelterVault, IShelterVaultThemeService shelterVaultThemeService, IShelterVaultStateService shelterVaultStateService, ILanguageService languageService, ICloudSyncManager cloudSyncManager, IUIThreadService uiThreadService, IWeakReferenceInstanceManager weakReferenceInstanceManager)
        {
            _shelterVaultThemeService = shelterVaultThemeService;
            _shelterVaultStateService = shelterVaultStateService;
            _languageService = languageService;
            _cloudSyncManager = cloudSyncManager;
            _uiThreadService = uiThreadService;
            _weakReferenceInstanceManager = weakReferenceInstanceManager;
            ShelterVaultCurrentAppState = ShelterVaultAppState.CreateMasterKey;
            InitialSetup(shelterVault);
        }

        [RelayCommand]
        private void SwitchVault()
        {
            _shelterVaultStateService.ResetState();
            WeakReferenceMessenger.Default.Send(new CurrentAppStateRequestMessage(ShelterVaultAppState.ConfirmMasterKey));
        }

        [RelayCommand]
        private void ChangeTheme()
        {
            CurrentAppTheme = _shelterVaultThemeService.GetNextTheme(CurrentAppTheme);
        }

        [RelayCommand]
        private void Sync()
        {
            if (ShelterVaultCurrentAppState == ShelterVaultAppState.NavigationView)
            {
                WeakReferenceMessenger.Default.Send(new ShowPageRequestMessage(ShelterVaultPage.SETTINGS));
            }
        }


        private void InitialSetup(IShelterVault shelterVault)
        {
            RegisterMessages();
            CurrentAppTheme = _shelterVaultThemeService.GetTheme();
            if (shelterVault.AreThereVaults()) ShelterVaultCurrentAppState = ShelterVaultAppState.ConfirmMasterKey;
            ShowLangOptions = true;
            ShowSwitchVault = false;
            SetLangText();
            StartSynchronizationTask();
        }

        private void RegisterMessages()
        {
            _weakReferenceInstanceManager.AddInstance(this);
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, CurrentAppStateRequestMessage>(this, (receiver, payload) =>
            {
                receiver.ShelterVaultCurrentAppState = payload.Value;
                RefreshSyncStatusInfo();
                if (payload.Value == ShelterVaultAppState.NavigationView)
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
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, UpdateLanguageValuesMessage>(this, (viewModel, payload) =>
            {
                viewModel.SetLangText();
            });
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, ProgressBarRequestMessage>(this, (viewModel, payload) =>
            {
                viewModel.IsProgressBarVisible = payload.Value;
            });
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, RefreshCurrentSyncStatusMessage>(this, (viewModel, payload) =>
            {
                viewModel._uiThreadService.Execute(() =>
                {
                    viewModel.CurrentCloudSyncStatus = payload.Value;
                });
            });
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, CloudProviderChangedMessage>(this, (viewModel, payload) =>
            {
                viewModel._uiThreadService.Execute(() =>
                {
                    CloudSyncInformation cloudSyncInformation = viewModel._cloudSyncManager.GetCurrentCloudSyncInformation();
                    viewModel.ShowSync = cloudSyncInformation.HasCloudConfiguration;
                    viewModel.CurrentCloudSyncStatus = cloudSyncInformation.CurrentSyncStatus;
                });
            });
            WeakReferenceMessenger.Default.Register<MainWindowViewModel, ShowSyncStatusMessage>(this, (viewModel, payload) =>
            {
                viewModel._uiThreadService.Execute(() =>
                {
                    if (payload.Value)
                        viewModel.RefreshSyncStatusInfo();
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
            if (ShelterVaultCurrentAppState == ShelterVaultAppState.NavigationView)
            {
                CloudSyncInformation cloudSyncInformation = _cloudSyncManager.GetCurrentCloudSyncInformation();
                ShowSync = cloudSyncInformation.HasCloudConfiguration;
                CurrentCloudSyncStatus = cloudSyncInformation.CurrentSyncStatus;
            }
            else
            {
                ShowSync = false;
                CurrentCloudSyncStatus = CloudSyncStatus.None;
            }
        }

        private void StartSynchronizationTask()
        {
            RefreshSyncStatusInfo();
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(60 * 1000);
                    if (ShelterVaultCurrentAppState == ShelterVaultAppState.NavigationView)
                    {
                        CloudSyncInformation cloudSyncInformation = _cloudSyncManager.GetCurrentCloudSyncInformation();
                        try
                        {
                            if (cloudSyncInformation.CanSynchronize)
                                await _cloudSyncManager.SyncVaults();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                        finally
                        {
                            if (cloudSyncInformation.CanSynchronize)
                            {
                                _uiThreadService.Execute(() =>
                                {
                                    RefreshSyncStatusInfo();
                                });
                            }
                        }
                    }
                }
            });
        }
    }
}
