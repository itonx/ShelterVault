using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Managers
{
    internal interface ICloudSyncManager
    {
        Task<bool> UpsertItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel;
        Task DeleteItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel;
        Task<ICosmosDBModel> GetItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel;
        Task SyncVaults();
        CloudSyncInformation GetCurrentCloudSyncInformation();
    }

    internal class CloudSyncManager : ICloudSyncManager
    {
        private readonly IShelterVaultCosmosDBService _shelterVaultCosmosDBService;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly ICloudProviderManager _cloudProviderManager;

        public CloudSyncManager(IShelterVaultCosmosDBService shelterVaultCosmosDBService, IShelterVaultLocalStorage shelterVaultLocalStorage, ICloudProviderManager cloudProviderManager)
        {
            _shelterVaultCosmosDBService = shelterVaultCosmosDBService;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _cloudProviderManager = cloudProviderManager;
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
            string uuidVault = _shelterVaultLocalStorage.GetCurrentUUIDVault();
            switch (providerType)
            {
                case CloudProviderType.Azure:
                    await _shelterVaultCosmosDBService.SyncAllAsync(uuidVault);
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
    }
}
