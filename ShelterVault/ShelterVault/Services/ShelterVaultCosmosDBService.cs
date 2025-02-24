using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Core;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Messages;
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
        Task SyncAllAsync(string uuidVault);
        Task<List<CosmosDBSyncModel>> SynchronizeModelsAsync(IList<CosmosDBSyncModel> cosmosDBSyncModels, IList<CosmosDBSyncModel> shelterVaultSyncModels);
        Task<CosmosDBTinyModel> GetItemByIdAsync(string id);
        CosmosDBSyncStatus GetCurrentSyncStatus();
    }

    public class ShelterVaultCosmosDBService : IShelterVaultCosmosDBService
    {
        private readonly IVaultReaderManager _vaultReaderManager;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly ICloudProviderManager _cloudProviderManager;

        public ShelterVaultCosmosDBService(IVaultReaderManager vaultReaderManager, IShelterVaultLocalStorage shelterVaultLocalStorage, ICloudProviderManager cloudProviderManager)
        {
            _vaultReaderManager = vaultReaderManager;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _cloudProviderManager = cloudProviderManager;
        }

        public async Task UpsertItemAsync<T>(T shelterVault) where T : ICosmosDBModel
        {
            ShelterVaultSyncStatusModel shelterVaultSyncStatusModel = _cloudProviderManager.GetSyncStatus(CloudProviderType.Azure);
            if (shelterVaultSyncStatusModel.IsSyncEnabled)
            {
                CosmosDBSettings cosmosDBSettings = _cloudProviderManager.GetCloudConfiguration<CosmosDBSettings>(CloudProviderType.Azure);
                using CosmosClient cosmosClient = new(accountEndpoint: cosmosDBSettings.CosmosEndpoint, authKeyOrResourceToken: cosmosDBSettings.CosmosKey);
                Database cosmosDb = cosmosClient.GetDatabase(cosmosDBSettings.CosmosDatabase);
                Container cosmosContainer = cosmosDb.GetContainer(cosmosDBSettings.CosmosContainer);
                ItemResponse<object> vaultResponse = await cosmosContainer.UpsertItemAsync(item: (object)shelterVault);
            }
        }

        public async Task DeleteItemAsync<T>(T shelterVault) where T : ICosmosDBModel
        {
            ShelterVaultSyncStatusModel shelterVaultSyncStatusModel = _cloudProviderManager.GetSyncStatus(CloudProviderType.Azure);
            if (shelterVaultSyncStatusModel.IsSyncEnabled)
            {
                CosmosDBSettings cosmosDBSettings = _cloudProviderManager.GetCloudConfiguration<CosmosDBSettings>(CloudProviderType.Azure);
                using CosmosClient cosmosClient = new(accountEndpoint: cosmosDBSettings.CosmosEndpoint, authKeyOrResourceToken: cosmosDBSettings.CosmosKey);
                Database cosmosDb = cosmosClient.GetDatabase(cosmosDBSettings.CosmosDatabase);
                Container cosmosContainer = cosmosDb.GetContainer(cosmosDBSettings.CosmosContainer);
                ItemResponse<object> vaultResponse = await cosmosContainer.DeleteItemAsync<object>(id: shelterVault.id, partitionKey: new PartitionKey(shelterVault.type));
            }
        }

        public async Task<CosmosDBTinyModel> GetItemByIdAsync(string id)
        {
            ShelterVaultSyncStatusModel shelterVaultSyncStatusModel = _cloudProviderManager.GetSyncStatus(CloudProviderType.Azure);
            if (!shelterVaultSyncStatusModel.IsSyncEnabled) return null;
            QueryDefinition query = new QueryDefinition("SELECT * FROM vault WHERE vault.id = @id")
            .WithParameter("@id", id);

            IList<CosmosDBTinyModel> results = await GetCosmosDBItems<CosmosDBTinyModel>(query);
            return results.FirstOrDefault();
        }

        public async Task SyncAllAsync(string uuidVault)
        {
            try
            {
                //TODO: look at this, lol (It's a POC)
                ShelterVaultSyncStatusModel shelterVaultSyncStatusModel = _cloudProviderManager.GetSyncStatus(CloudProviderType.Azure);
                if (!shelterVaultSyncStatusModel.IsSyncEnabled) return;
                WeakReferenceMessenger.Default.Send(new RefreshCurrentSyncStatusMessage(Shared.Enums.CloudSyncStatus.SynchInProcess));
                string uuidVaultClause = $" WHERE ((vault.type = 'shelter_vault' and vault.id='{uuidVault}') or (vault.type = 'shelter_vault_credentials' and vault.shelterVaultUuid = '{uuidVault}'))";
                string timestampClause = shelterVaultSyncStatusModel.Timestamp != 0 ? $" and vault._ts > {shelterVaultSyncStatusModel.Timestamp}" : string.Empty;
                string cosmosDBquery = string.Concat("SELECT vault.id, vault.type, vault.version FROM vault", uuidVaultClause, timestampClause);
                QueryDefinition queryDefinition = new QueryDefinition(cosmosDBquery);
                IList<CosmosDBTinyModel> cosmosDBTinyModels = await GetCosmosDBItems<CosmosDBTinyModel>(queryDefinition);
                IList<VaultModel> shelterVaults = _vaultReaderManager.GetCurrentVault();
                IList<CosmosDBSyncModel> cosmosDBModels = CosmosDBTinyModel.ToCosmosDBSyncModel(cosmosDBTinyModels);
                IList<CosmosDBSyncModel> localDBVaults  = VaultModel.ToCosmosDBSyncModel(shelterVaults);
                List<CosmosDBSyncModel> synchronizedModels = await SynchronizeModelsAsync(cosmosDBModels, localDBVaults);

                foreach (var model in await GetCosmosDBItemsByPartitionKey(synchronizedModels, "shelter_vault"))
                {
                    CosmosDBVault cosmosDBVault = (CosmosDBVault)model;
                    CosmosDBSyncModel cosmosDBModel = cosmosDBModels.FirstOrDefault(x => x.id.Equals(cosmosDBVault.id));
                    if(cosmosDBModel != null && cosmosDBModel.IsNew) _shelterVaultLocalStorage.CreateShelterVault(cosmosDBVault.id, cosmosDBVault.name, cosmosDBVault.masterKeyHash, cosmosDBVault.iv, cosmosDBVault.salt, cosmosDBVault.version);
                    else _shelterVaultLocalStorage.UpdateShelterVault(cosmosDBVault.id, cosmosDBVault.name, cosmosDBVault.masterKeyHash, cosmosDBVault.iv, cosmosDBVault.salt, cosmosDBVault.version);
                }

                foreach (var model in await GetCosmosDBItemsByPartitionKey(synchronizedModels, "shelter_vault_credentials"))
                {
                    CosmosDBCredentials cosmosDBCredentials = (CosmosDBCredentials)model;
                    CosmosDBSyncModel cosmosDBModel = cosmosDBModels.FirstOrDefault(x => x.id.Equals(cosmosDBCredentials.id));
                    if (cosmosDBModel != null && cosmosDBModel.IsNew) _shelterVaultLocalStorage.InsertCredentials(new(cosmosDBCredentials));
                    else _shelterVaultLocalStorage.UpdateCredentials(new(cosmosDBCredentials));
                }

                foreach(var model in synchronizedModels.Where(x => x.source == SourceType.Local && x.type == "shelter_vault"))
                {
                    ShelterVaultModel vault = _shelterVaultLocalStorage.GetVaultByUUID(model.id);
                    ICosmosDBModel cosmosDBModel = vault.ToCosmosDBModel();
                    await UpsertItemAsync(cosmosDBModel);
                }

                foreach (var model in synchronizedModels.Where(x => x.source == SourceType.Local && x.type == "shelter_vault_credentials"))
                {
                    ShelterVaultCredentialsModel credentials = _shelterVaultLocalStorage.GetCredentialsByUUID(model.id);
                    ICosmosDBModel cosmosDBModel = credentials.ToCosmosDBModel();
                    await UpsertItemAsync(cosmosDBModel);
                }

                _cloudProviderManager.UpdateSyncTimestamp(CloudProviderType.Azure, DateTimeOffset.Now.ToUnixTimeSeconds());
                foreach (var model in synchronizedModels.Where(x => x.source == SourceType.Local && x.version == -1 && x.type.Equals("shelter_vault_credentials")))
                {
                    _shelterVaultLocalStorage.DeleteCredentials(model.id);
                }

                foreach (var model in synchronizedModels.Where(x => x.source == SourceType.Local && x.version == -1 && x.type.Equals("shelter_vault")))
                {
                    _shelterVaultLocalStorage.DeleteVault(model.id);
                }

                WeakReferenceMessenger.Default.Send(new RefreshCredentialListRequestMessage(true));
                WeakReferenceMessenger.Default.Send(new RefreshVaultListRequestMessage(true));
                
                _cloudProviderManager.UpdateSyncStatus(CloudProviderType.Azure, CloudSyncStatus.UpToDate);
            }
            catch (Exception ex)
            {
                _cloudProviderManager.UpdateSyncStatus(CloudProviderType.Azure, CloudSyncStatus.SynchFailed);
                throw ex;
            }
        }

        private async Task<IEnumerable<ICosmosDBModel>> GetCosmosDBItemsByPartitionKey(IList<CosmosDBSyncModel> cosmosDBModels, string partitionKey)
        {
            if(cosmosDBModels.Count == 0) return Enumerable.Empty<ICosmosDBModel>();

            string baseQuery = partitionKey.Equals("shelter_vault") ? "SELECT vault.name, vault.masterKeyHash, vault.iv, vault.salt, vault.id, vault.type, vault.version FROM vault"
                : "SELECT vault.encryptedValues, vault.iv, vault.shelterVaultUuid, vault.id, vault.type, vault.version FROM vault";

            QueryDefinition query = new QueryDefinition(string.Concat(baseQuery, " where ARRAY_CONTAINS(@ids, vault.id) and vault.type = @type"))
                .WithParameter("@ids", cosmosDBModels.Where(x => x.source == SourceType.CosmosDB && x.type.Equals(partitionKey)).Select(x => x.id))
                .WithParameter("@type", partitionKey);

            return partitionKey.Equals("shelter_vault") ? await GetCosmosDBItems<CosmosDBVault>(query) : await GetCosmosDBItems<CosmosDBCredentials>(query);
        }

        private async Task<IList<T>> GetCosmosDBItems<T>(QueryDefinition query) where T : ICosmosDBModel
        {
            CosmosDBSettings cosmosDBSettings = _cloudProviderManager.GetCloudConfiguration<CosmosDBSettings>(CloudProviderType.Azure);
            using CosmosClient cosmosClient = new(accountEndpoint: cosmosDBSettings.CosmosEndpoint, authKeyOrResourceToken: cosmosDBSettings.CosmosKey);
            Database cosmosDb = cosmosClient.GetDatabase(cosmosDBSettings.CosmosDatabase);
            Container cosmosContainer = cosmosDb.GetContainer(cosmosDBSettings.CosmosContainer);
            IList<T> results = new List<T>();

            using FeedIterator<T> feed = cosmosContainer.GetItemQueryIterator<T>(
                queryDefinition: query
            );

            while (feed.HasMoreResults)
            {
                FeedResponse<T> response = await feed.ReadNextAsync();
                foreach (T item in response) results.Add(item);
            }

            return results;
        }

        public async Task<List<CosmosDBSyncModel>> SynchronizeModelsAsync(IList<CosmosDBSyncModel> cosmosDBSyncModels, IList<CosmosDBSyncModel> shelterVaultSyncModels)
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
                        else if(cosmosDBItem.Value.version == -1)
                            syncModels.Add(cosmosDBItem.Value);
                        else if(shelterVaultItems[cosmosDBItem.Key].version == -1)
                            syncModels.Add(shelterVaultItems[cosmosDBItem.Key]);
                        else if(shelterVaultItems[cosmosDBItem.Key].version < cosmosDBItem.Value.version)
                            syncModels.Add(cosmosDBItem.Value);
                        else
                            syncModels.Add(shelterVaultItems[cosmosDBItem.Key]);
                    }
                    else
                    {
                        cosmosDBItem.Value.IsNew = true;
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

        public CosmosDBSyncStatus GetCurrentSyncStatus()
        {
            ShelterVaultSyncStatusModel shelterVaultSyncStatusModel = _cloudProviderManager.GetSyncStatus(CloudProviderType.Azure);
            CosmosDBSyncStatus currentSyncStatus = new(shelterVaultSyncStatusModel);
            return currentSyncStatus;
        }
    }
}
