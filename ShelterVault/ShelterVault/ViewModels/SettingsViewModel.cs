﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace ShelterVault.ViewModels
{
    internal partial class SettingsViewModel : ObservableObject
    {
        private readonly IDialogManager _dialogManager;
        private readonly IProgressBarService _progressBarService;
        private readonly ICloudProviderManager _cloudProviderManager;
        private readonly IShelterVaultSyncStatus _shelterVaultSyncStatus;
        private readonly ICloudSyncManager _cloudSyncManager;
        private readonly ILogger<SettingsViewModel> _logger;

        [ObservableProperty]
        private IList<CloudProviderType> _cloudProviders;
        [ObservableProperty]
        private CloudProviderType _selectedCloudProvider;
        [ObservableProperty]
        private string _cosmosEndpoint;
        [ObservableProperty]
        private string _cosmosKey;
        [ObservableProperty]
        private string _cosmosDatabase;
        [ObservableProperty]
        private string _cosmosContainer;
        [ObservableProperty]
        private bool _showThroughput;
        [ObservableProperty]
        private string _databaseThroughput;
        [ObservableProperty]
        private string _containerPartitionKey;
        [ObservableProperty]
        private string _appVersion;

        public SettingsViewModel(IDialogManager dialogManager, IProgressBarService progressBarService, ICloudProviderManager cloudProviderManager, IShelterVaultSyncStatus shelterVaultSyncStatus, ILogger<SettingsViewModel> logger, ICloudSyncManager cloudSyncManager)
        {
            _dialogManager = dialogManager;
            _progressBarService = progressBarService;
            _cloudProviderManager = cloudProviderManager;
            _shelterVaultSyncStatus = shelterVaultSyncStatus;
            _cloudSyncManager = cloudSyncManager;
            _logger = logger;
            AppVersion = GetAppVersion();
            CloudProviders = new List<CloudProviderType>((CloudProviderType[])Enum.GetValues(typeof(CloudProviderType)));
            SelectedCloudProvider = _cloudProviderManager.GetCurrentCloudProvider();
            ReadCosmosDBSettings();
        }

        private string GetAppVersion()
        {
            return string.Format("v{0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);
        }

        partial void OnSelectedCloudProviderChanged(CloudProviderType value)
        {
            if (value == CloudProviderType.None)
            {
                _shelterVaultSyncStatus.DisableSync(CloudProviderType.Azure);
                ShowThroughput = false;
                ReadCosmosDBSettings();
            }
            _cloudProviderManager.UpdateVaultCloudProvider(value);
            WeakReferenceMessenger.Default.Send(new CloudProviderChangedMessage(true));
        }

        private void ReadCosmosDBSettings()
        {

            CosmosDBSettings cosmosDBSettings = _cloudProviderManager.GetCloudConfiguration<CosmosDBSettings>(CloudProviderType.Azure);
            if (cosmosDBSettings == null) return;
            CosmosEndpoint = cosmosDBSettings.CosmosEndpoint;
            CosmosKey = cosmosDBSettings.CosmosKey;
            CosmosDatabase = cosmosDBSettings.CosmosDatabase;
            CosmosContainer = cosmosDBSettings.CosmosContainer;
        }

        [RelayCommand]
        private async Task TestConnection()
        {
            try
            {
                await _progressBarService.Show();
                ShowThroughput = false;
                if (SelectedCloudProvider == CloudProviderType.Azure)
                {
                    CosmosDBSettings cosmosDBSettings = new(CosmosEndpoint, CosmosKey, CosmosDatabase, CosmosContainer);
                    if (cosmosDBSettings.IsValid())
                    {
                        try
                        {
                            using CosmosClient cosmosClient = new(accountEndpoint: CosmosEndpoint, authKeyOrResourceToken: CosmosKey);
                            Database cosmosDb = cosmosClient.GetDatabase(CosmosDatabase);
                            Container cosmosContainer = cosmosDb.GetContainer(CosmosContainer);
                            int? databaseThroughput = await cosmosDb.ReadThroughputAsync();
                            ContainerResponse containerResponse = await cosmosContainer.ReadContainerAsync();

                            ShowThroughput = true;

                            DatabaseThroughput = databaseThroughput?.ToString() ?? "Err";
                            ContainerPartitionKey = containerResponse?.Resource?.PartitionKeyPath ?? "Err";

                            _cloudProviderManager.UpsertCloudConfiguration(CloudProviderType.Azure, cosmosDBSettings);
                            _shelterVaultSyncStatus.UpsertSyncStatus(CloudProviderType.Azure, 0, true, CloudSyncStatus.PendingConfiguration);

                        }
                        catch
                        {
                            _shelterVaultSyncStatus.UpsertSyncStatus(CloudProviderType.Azure, 0, false, CloudSyncStatus.None);
                            await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_COSMOS_DB_SETTINGS_TEST_ERROR);
                        }
                        finally
                        {
                            WeakReferenceMessenger.Default.Send(new ShowSyncStatusMessage(true));
                        }
                    }
                }
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }

        [RelayCommand]
        private async Task SyncVaults()
        {
            try
            {
                await _progressBarService.Show();
                await _cloudSyncManager.SyncVaults();
                await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_COSMOS_DB_SYNC_DONE);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing vaults");
                await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_COSMOS_DB_SYNC_ERROR);
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }
    }
}
