using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Messages;
using System;
using System.Threading.Tasks;

namespace ShelterVault.Managers
{
    public interface ICloudSyncManager
    {
        Task<bool> UpsertItemAsync<T>(T shelterVaultModel, bool validateItem = false) where T : IShelterVaultLocalModel;
        Task DeleteItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel;
        Task<ICosmosDBModel> GetItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel;
        Task SyncVaults();
        CloudSyncInformation GetCurrentCloudSyncInformation();
    }

    public class CloudSyncManager : ICloudSyncManager
    {
        private readonly IShelterVaultCosmosDBService _shelterVaultCosmosDBService;
        private readonly ICloudProviderManager _cloudProviderManager;
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultSyncStatus _shelterVaultSyncStatus;
        private readonly ILogger<CloudSyncManager> _logger;
        public readonly IShelterVaultStateService _shelterVaultStateService;

        public CloudSyncManager(IShelterVaultCosmosDBService shelterVaultCosmosDBService, ICloudProviderManager cloudProviderManager, IShelterVault shelterVault, IShelterVaultSyncStatus shelterVaultSyncStatus, ILogger<CloudSyncManager> logger, IShelterVaultStateService shelterVaultStateService)
        {
            _shelterVaultCosmosDBService = shelterVaultCosmosDBService;
            _cloudProviderManager = cloudProviderManager;
            _shelterVault = shelterVault;
            _shelterVaultSyncStatus = shelterVaultSyncStatus;
            _logger = logger;
            _shelterVaultStateService = shelterVaultStateService;
        }

        public async Task<ICosmosDBModel> GetItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel
        {
            try
            {
                CloudProviderType providerType = _cloudProviderManager.GetCurrentCloudProvider();
                ICosmosDBModel result = null;
                switch (providerType)
                {
                    case CloudProviderType.Azure:
                        ICosmosDBModel model = shelterVaultModel.ToCosmosDBModel();
                        result = await _shelterVaultCosmosDBService.GetItemByIdAsync(model.id);
                        break;
                    default:
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item from cloud");
                return default(ICosmosDBModel);
            }
        }

        public async Task<bool> UpsertItemAsync<T>(T shelterVaultModel, bool validateItem = false) where T : IShelterVaultLocalModel
        {
            try
            {
                CloudProviderType providerType = _cloudProviderManager.GetCurrentCloudProvider();
                switch (providerType)
                {
                    case CloudProviderType.Azure:
                        ICosmosDBModel cosmosDBVault = shelterVaultModel.ToCosmosDBModel();
                        if (validateItem && !await _shelterVaultCosmosDBService.CanAffectItemAsync(cosmosDBVault.id)) return true;
                        await _shelterVaultCosmosDBService.UpsertItemAsync(cosmosDBVault);
                        break;
                    default:
                        break;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task DeleteItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel
        {
            CloudProviderType providerType = _cloudProviderManager.GetCurrentCloudProvider();
            switch (providerType)
            {
                case CloudProviderType.Azure:
                    ICosmosDBModel cosmosDBVault = shelterVaultModel.ToCosmosDBModel();
                    await _shelterVaultCosmosDBService.DeleteItemAsync(cosmosDBVault);
                    break;
                default:
                    break;
            }
        }

        public async Task SyncVaults()
        {
            CloudProviderType providerType = _cloudProviderManager.GetCurrentCloudProvider();
            ShelterVaultModel shelterVaultModel = _shelterVault.GetCurrentVault();
            switch (providerType)
            {
                case CloudProviderType.Azure:
                    try
                    {
                        await _shelterVaultCosmosDBService.SyncAllAsync(shelterVaultModel.UUID);
                        _shelterVaultSyncStatus.UpdateSyncStatus(CloudProviderType.Azure, CloudSyncStatus.UpToDate);
                        WeakReferenceMessenger.Default.Send(new RefreshCurrentSyncStatusMessage(CloudSyncStatus.UpToDate));
                    }
                    catch (Exception)
                    {
                        _shelterVaultSyncStatus.UpdateSyncStatus(CloudProviderType.Azure, CloudSyncStatus.SynchFailed);
                        WeakReferenceMessenger.Default.Send(new RefreshCurrentSyncStatusMessage(CloudSyncStatus.SynchFailed));
                        throw;
                    }
                    break;
                default:
                    break;
            }
        }

        CloudSyncInformation ICloudSyncManager.GetCurrentCloudSyncInformation()
        {
            CloudProviderType providerType = _cloudProviderManager.GetCurrentCloudProvider();
            ShelterVaultSyncStatusModel model = _shelterVaultSyncStatus.GetSyncStatus(providerType);
            bool isDialogOpen = _shelterVaultStateService.GetDialogStatus();
            return new(model, isDialogOpen);
        }
    }
}
