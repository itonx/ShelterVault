using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Core;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Services
{
    internal interface IShelterVaultCosmosDBService
    {
        Task UpsertItemAsync<T>(T shelterVault) where T : ICosmosDBModel;
        Task DeleteItemAsync<T>(T shelterVault) where T : ICosmosDBModel;
        Task SyncAllAsync();
    }

    internal class ShelterVaultCosmosDBService : IShelterVaultCosmosDBService
    {
        private readonly ISettingsService _settingsService;
        private readonly IVaultReaderManager _vaultReaderManager;

        public ShelterVaultCosmosDBService(ISettingsService settingsService, IVaultReaderManager vaultReaderManager)
        {
            _settingsService = settingsService;
            _vaultReaderManager = vaultReaderManager;
        }

        public async Task UpsertItemAsync<T>(T shelterVault) where T : ICosmosDBModel
        {
            CosmosDBSettings cosmosDBSettings = _settingsService.ReadJsonValueAs<CosmosDBSettings>(ShelterVaultConstants.COSMOS_DB_SETTINGS);
            using CosmosClient cosmosClient = new(accountEndpoint: cosmosDBSettings.CosmosEndpoint, authKeyOrResourceToken: cosmosDBSettings.CosmosKey);
            Database cosmosDb = cosmosClient.GetDatabase(cosmosDBSettings.CosmosDatabase);
            Container cosmosContainer = cosmosDb.GetContainer(cosmosDBSettings.CosmosContainer);
            ItemResponse<T> vaultResponse = await cosmosContainer.UpsertItemAsync(item: shelterVault);
        }

        public async Task DeleteItemAsync<T>(T shelterVault) where T : ICosmosDBModel
        {
            CosmosDBSettings cosmosDBSettings = _settingsService.ReadJsonValueAs<CosmosDBSettings>(ShelterVaultConstants.COSMOS_DB_SETTINGS);
            using CosmosClient cosmosClient = new(accountEndpoint: cosmosDBSettings.CosmosEndpoint, authKeyOrResourceToken: cosmosDBSettings.CosmosKey);
            Database cosmosDb = cosmosClient.GetDatabase(cosmosDBSettings.CosmosDatabase);
            Container cosmosContainer = cosmosDb.GetContainer(cosmosDBSettings.CosmosContainer);
            ItemResponse<object> vaultResponse = await cosmosContainer.DeleteItemAsync<object>(id: shelterVault.id, partitionKey: new PartitionKey(shelterVault.type));
        }

        public async Task SyncAllAsync()
        {
            CosmosDBSettings cosmosDBSettings = _settingsService.ReadJsonValueAs<CosmosDBSettings>(ShelterVaultConstants.COSMOS_DB_SETTINGS);
            IList<VaultModel> vaults = _vaultReaderManager.GetAllVaults();
            using CosmosClient cosmosClient = new(accountEndpoint: cosmosDBSettings.CosmosEndpoint, authKeyOrResourceToken: cosmosDBSettings.CosmosKey);
            Database cosmosDb = cosmosClient.GetDatabase(cosmosDBSettings.CosmosDatabase);
            Container cosmosContainer = cosmosDb.GetContainer(cosmosDBSettings.CosmosContainer);

            foreach (VaultModel vault in vaults)
            {
                ICosmosDBModel cosmosDBVault = vault.ShelterVault.ToCosmosDBModel();
                ItemResponse<ICosmosDBModel> vaultResponse = await cosmosContainer.UpsertItemAsync(item: cosmosDBVault);
                foreach (var credential in vault.ShelterVaultCredentials)
                {
                    ItemResponse<ICosmosDBModel> credentialsResponse = await cosmosContainer.UpsertItemAsync(item: credential.ToCosmosDBModel());
                }
            }
        }
    }
}
