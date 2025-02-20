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
    public interface IShelterVaultCosmosDBService
    {
        Task UpsertItemAsync<T>(T shelterVault) where T : ICosmosDBModel;
        Task DeleteItemAsync<T>(T shelterVault) where T : ICosmosDBModel;
        Task SyncAllAsync();
        Task<List<CosmosDBSyncModel>> GetSynchronizedModelsAsync(IList<CosmosDBSyncModel> cosmosDBSyncModels, IList<CosmosDBSyncModel> shelterVaultSyncModels);
    }

    public class ShelterVaultCosmosDBService : IShelterVaultCosmosDBService
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
            ItemResponse<object> vaultResponse = await cosmosContainer.UpsertItemAsync(item: (object)shelterVault);
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
            //TODO: Refactor this method to use GetSynchronizedModelsAsync
            try
            {
                CosmosDBSettings cosmosDBSettings = _settingsService.ReadJsonValueAs<CosmosDBSettings>(ShelterVaultConstants.COSMOS_DB_SETTINGS);
                IList<VaultModel> vaults = _vaultReaderManager.GetAllVaults();
                using CosmosClient cosmosClient = new(accountEndpoint: cosmosDBSettings.CosmosEndpoint, authKeyOrResourceToken: cosmosDBSettings.CosmosKey);
                Database cosmosDb = cosmosClient.GetDatabase(cosmosDBSettings.CosmosDatabase);
                Container cosmosContainer = cosmosDb.GetContainer(cosmosDBSettings.CosmosContainer);

                var query = new QueryDefinition(
                    query: "SELECT * FROM ShelterVaultContainer v"
                );

                using FeedIterator<object> feed = cosmosContainer.GetItemQueryIterator<object>(
                    queryDefinition: query
                );

                while (feed.HasMoreResults)
                {
                    FeedResponse<object> response = await feed.ReadNextAsync();

                    foreach (dynamic item in response)
                    {
                        if (item.type.Equals("shelter_vault"))
                        {
                            ShelterVaultModel shelterVaultModel = vaults.FirstOrDefault(v => v.ShelterVault.UUID.Equals(item.id))?.ShelterVault;
                        }
                        else if (item.type.Equals("shelter_vault_credentials"))
                        {
                            CosmosDBCredentials credentials = (CosmosDBCredentials)item;
                            ShelterVaultCredentialsModel shelterVaultCredentialsModel = vaults.FirstOrDefault(v => v.ShelterVault.UUID.Equals(credentials.shelterVaultUuid))?.ShelterVaultCredentials?.FirstOrDefault(c => c.Equals(credentials.id));
                        }
                    }

                    foreach (VaultModel vault in vaults)
                    {
                        ICosmosDBModel cosmosDBVault = vault.ShelterVault.ToCosmosDBModel();
                        ItemResponse<object> vaultResponse = await cosmosContainer.UpsertItemAsync(item: (object)cosmosDBVault);
                        foreach (var credential in vault.ShelterVaultCredentials)
                        {
                            ItemResponse<object> credentialsResponse = await cosmosContainer.UpsertItemAsync(item: (object)credential.ToCosmosDBModel());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<CosmosDBSyncModel>> GetSynchronizedModelsAsync(IList<CosmosDBSyncModel> cosmosDBSyncModels, IList<CosmosDBSyncModel> shelterVaultSyncModels)
        {
            return await Task.Run(() =>
            {
                Dictionary<string, CosmosDBSyncModel> cosmosDBItems = cosmosDBSyncModels.ToDictionary(x => x.id, x => x);
                Dictionary<string, CosmosDBSyncModel> shelterVaultItems = shelterVaultSyncModels.ToDictionary(x => x.id, x => x);
                List<CosmosDBSyncModel> syncModels = new();

                foreach (KeyValuePair<string, CosmosDBSyncModel> cosmosDBItem in cosmosDBItems)
                {
                    if (shelterVaultItems.ContainsKey(cosmosDBItem.Key))
                    {
                        if (shelterVaultItems[cosmosDBItem.Key].version == cosmosDBItem.Value.version) continue;
                        if(shelterVaultItems[cosmosDBItem.Key].version < cosmosDBItem.Value.version)
                            syncModels.Add(cosmosDBItem.Value);
                        else
                            syncModels.Add(shelterVaultItems[cosmosDBItem.Key]);
                    }
                    else
                    {
                        syncModels.Add(cosmosDBItem.Value);
                    }
                }

                foreach (KeyValuePair<string, CosmosDBSyncModel> shelterVaultItem in shelterVaultItems)
                {
                    if (!cosmosDBItems.ContainsKey(shelterVaultItem.Key))
                    {
                        syncModels.Add(shelterVaultItem.Value);
                    }
                }

                return syncModels;
            });
        }
    }
}
