using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Azure.Cosmos;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IProgressBarService _progressBarService;
        private readonly IShelterVaultCosmosDBService _shelterVaultCosmosDBService;

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

        public SettingsViewModel(ISettingsService settingsService, IDialogService dialogService, IProgressBarService progressBarService, IShelterVaultCosmosDBService shelterVaultCosmosDBService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _progressBarService = progressBarService;
            _shelterVaultCosmosDBService = shelterVaultCosmosDBService;
            CloudProviders = new List<CloudProviderType>((CloudProviderType[])Enum.GetValues(typeof(CloudProviderType)));
            SelectedCloudProvider = _settingsService.GetCurrentCloudProviderType();
            ReadCosmosDBSettings();
        }

        partial void OnSelectedCloudProviderChanged(CloudProviderType value)
        {
            _settingsService.SaveCloudProviderType(value);
        }

        private void ReadCosmosDBSettings()
        {
            CosmosDBSettings cosmosDBSettings = _settingsService.ReadJsonValueAs<CosmosDBSettings>(ShelterVaultConstants.COSMOS_DB_SETTINGS);
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

                            _settingsService.SaveAsJsonValue(ShelterVaultConstants.COSMOS_DB_SETTINGS, cosmosDBSettings);
                        }
                        catch
                        {
                            await _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_COSMOS_DB_SETTINGS_TEST_ERROR);
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
                await _shelterVaultCosmosDBService.SyncAllAsync();
                _settingsService.SaveAsJsonValue(ShelterVaultConstants.COSMOS_DB_SYNC_STATUS, new CosmosDBSyncStatus(true));
                await _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_COSMOS_DB_SYNC_DONE);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_COSMOS_DB_SYNC_ERROR);
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }
    }
}
