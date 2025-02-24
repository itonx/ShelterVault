using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Azure.Cosmos;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Messages;
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
        private readonly ICloudProviderManager _cloudProviderManager;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;

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

        public SettingsViewModel(ISettingsService settingsService, IDialogService dialogService, IProgressBarService progressBarService, IShelterVaultCosmosDBService shelterVaultCosmosDBService, ICloudProviderManager cloudProviderManager, IShelterVaultLocalStorage shelterVaultLocalStorage)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _progressBarService = progressBarService;
            _shelterVaultCosmosDBService = shelterVaultCosmosDBService;
            _cloudProviderManager = cloudProviderManager;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            CloudProviders = new List<CloudProviderType>((CloudProviderType[])Enum.GetValues(typeof(CloudProviderType)));
            SelectedCloudProvider = _cloudProviderManager.GetCurrentCloudProvider();
            ReadCosmosDBSettings();
        }

        partial void OnSelectedCloudProviderChanged(CloudProviderType value)
        {
            if(value == CloudProviderType.None)
            {
                _cloudProviderManager.DisableSync(CloudProviderType.Azure);
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
                            _cloudProviderManager.UpsertSyncStatus(CloudProviderType.Azure, 0, true, CloudSyncStatus.PendingConfiguration);

                        }
                        catch
                        {
                            _cloudProviderManager.UpsertSyncStatus(CloudProviderType.Azure, 0, false, CloudSyncStatus.None);
                            await _dialogService.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_COSMOS_DB_SETTINGS_TEST_ERROR);
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
                string uuidVault = _shelterVaultLocalStorage.GetCurrentUUIDVault();
                switch (SelectedCloudProvider)
                {
                    case CloudProviderType.Azure:
                        try
                        {
                            await _shelterVaultCosmosDBService.SyncAllAsync(uuidVault);
                            _cloudProviderManager.UpdateSyncStatus(CloudProviderType.Azure, CloudSyncStatus.UpToDate);
                            WeakReferenceMessenger.Default.Send(new RefreshCurrentSyncStatusMessage(Shared.Enums.CloudSyncStatus.UpToDate));
                        }
                        catch (Exception)
                        {
                            _cloudProviderManager.UpdateSyncStatus(CloudProviderType.Azure, CloudSyncStatus.SynchFailed);
                            WeakReferenceMessenger.Default.Send(new RefreshCurrentSyncStatusMessage(Shared.Enums.CloudSyncStatus.SynchFailed));
                            throw;
                        }
                        break;
                    default:
                        return;
                }
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
