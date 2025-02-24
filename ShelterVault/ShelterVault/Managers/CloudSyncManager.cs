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
        private readonly ISettingsService _settingsService;
        private readonly IShelterVaultCosmosDBService _shelterVaultCosmosDBService;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;

        public CloudSyncManager(ISettingsService settingsService, IShelterVaultCosmosDBService shelterVaultCosmosDBService, IShelterVaultLocalStorage shelterVaultLocalStorage)
        {
            _settingsService = settingsService;
            _shelterVaultCosmosDBService = shelterVaultCosmosDBService;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
        }

        public async Task<ICosmosDBModel> GetItemAsync<T>(T shelterVaultModel) where T : IShelterVaultLocalModel
        {
            try
            {
                CloudProviderType providerType = _settingsService.GetCurrentCloudProviderType();
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
                CloudProviderType providerType = _settingsService.GetCurrentCloudProviderType();
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
            CloudProviderType providerType = _settingsService.GetCurrentCloudProviderType();
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
            CloudProviderType providerType = _settingsService.GetCurrentCloudProviderType();
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
            CloudProviderType providerType = _settingsService.GetCurrentCloudProviderType();

            switch (providerType)
            {
                case CloudProviderType.Azure:
                    return new(_shelterVaultCosmosDBService.GetCurrentSyncStatus());
                default:
                    return new();
            }
        }
    }
}
