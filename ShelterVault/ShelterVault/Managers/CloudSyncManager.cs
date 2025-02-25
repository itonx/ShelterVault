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
        Task<bool> UpsertItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel;
        Task DeleteItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel;
        Task<ICosmosDBModel> GetItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel;
        Task SyncVaults();
        CloudSyncInformation GetCurrentCloudSyncInformation();
        ShelterVaultSyncStatusModel GetSyncStatus(CloudProviderType cloudProviderType);
        bool DisableSync(CloudProviderType cloudProviderType);
        bool UpdateSyncTimestamp(CloudProviderType cloudProviderType, long timestamp);
        bool UpdateSyncStatus(CloudProviderType cloudProviderType, CloudSyncStatus cloudSyncStatus);
        bool UpsertSyncStatus(CloudProviderType cloudProviderType, long timestamp, bool isSyncEnabled, CloudSyncStatus cloudSyncStatus);
    }

    public class CloudSyncManager : ICloudSyncManager
    {
        private readonly IShelterVaultCosmosDBService _shelterVaultCosmosDBService;
        private readonly ICloudProviderManager _cloudProviderManager;
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultSyncStatus _shelterVaultSyncStatus;
        private readonly ILogger<CloudSyncManager> _logger;

        public CloudSyncManager(IShelterVaultCosmosDBService shelterVaultCosmosDBService, ICloudProviderManager cloudProviderManager, IShelterVault shelterVault, IShelterVaultSyncStatus shelterVaultSyncStatus, ILogger<CloudSyncManager> logger)
        {
            _shelterVaultCosmosDBService = shelterVaultCosmosDBService;
            _cloudProviderManager = cloudProviderManager;
            _shelterVault = shelterVault;
            _shelterVaultSyncStatus = shelterVaultSyncStatus;
            _logger = logger;
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

        public async Task<bool> UpsertItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel
        {
            try
            {
                CloudProviderType providerType = _cloudProviderManager.GetCurrentCloudProvider();
                switch (providerType)
                {
                    case CloudProviderType.Azure:
                        ICosmosDBModel cosmosDBVault = shelterVaultModel.ToCosmosDBModel();
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
                        UpdateSyncStatus(CloudProviderType.Azure, CloudSyncStatus.UpToDate);
                        WeakReferenceMessenger.Default.Send(new RefreshCurrentSyncStatusMessage(Shared.Enums.CloudSyncStatus.UpToDate));
                    }
                    catch (Exception)
                    {
                        UpdateSyncStatus(CloudProviderType.Azure, CloudSyncStatus.SynchFailed);
                        WeakReferenceMessenger.Default.Send(new RefreshCurrentSyncStatusMessage(Shared.Enums.CloudSyncStatus.SynchFailed));
                        throw;
                    }
                    break;
                default:
                    break;
            }
        }

        public CloudSyncInformation GetCurrentCloudSyncInformation()
        {
            try
            {
                CloudProviderType providerType = _cloudProviderManager.GetCurrentCloudProvider();

                switch (providerType)
                {
                    case CloudProviderType.Azure:
                        return new(_shelterVaultCosmosDBService.GetCurrentSyncStatus());
                    default:
                        return new();
                }
            }
            catch (Exception)
            {
                return new();
            }
        }

        public ShelterVaultSyncStatusModel GetSyncStatus(CloudProviderType cloudProviderType)
        {
            return _shelterVaultSyncStatus.GetSyncStatus(cloudProviderType.ToString());
        }

        public bool UpsertSyncStatus(CloudProviderType cloudProviderType, long timestamp, bool isSyncEnabled, CloudSyncStatus cloudSyncStatus)
        {
            return _shelterVaultSyncStatus.UpsertSyncStatus(cloudProviderType.ToString(), timestamp, isSyncEnabled, (int)cloudSyncStatus);
        }

        public bool DisableSync(CloudProviderType cloudProviderType)
        {
            return UpsertSyncStatus(cloudProviderType, 0, false, CloudSyncStatus.None);
        }

        public bool UpdateSyncTimestamp(CloudProviderType cloudProviderType, long timestamp)
        {
            return _shelterVaultSyncStatus.UpdateSyncStatus(cloudProviderType.ToString(), timestamp);
        }

        public bool UpdateSyncStatus(CloudProviderType cloudProviderType, CloudSyncStatus cloudSyncStatus)
        {
            return _shelterVaultSyncStatus.UpdateSyncStatus(cloudProviderType.ToString(), cloudSyncStatus);
        }
    }
}
