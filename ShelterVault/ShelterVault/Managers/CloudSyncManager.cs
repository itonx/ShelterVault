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
    }

    internal class CloudSyncManager : ICloudSyncManager
    {
        private readonly ISettingsService _settingsService;
        private readonly IShelterVaultCosmosDBService _shelterVaultCosmosDBService;

        public CloudSyncManager(ISettingsService settingsService, IShelterVaultCosmosDBService shelterVaultCosmosDBService)
        {
            _settingsService = settingsService;
            _shelterVaultCosmosDBService = shelterVaultCosmosDBService;
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
    }
}
