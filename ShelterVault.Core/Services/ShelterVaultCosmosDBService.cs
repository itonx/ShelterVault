using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShelterVault.Services
{
    public interface IShelterVaultCosmosDBService
    {
        Task UpsertItemAsync<T>(T shelterVault) where T : ICosmosDBModel;
        Task DeleteItemAsync<T>(T shelterVault) where T : ICosmosDBModel;
        Task SyncAllAsync(string uuidVault);
        Task<CosmosDBTinyModel> GetItemByIdAsync(string id);
        CosmosDBSyncStatus GetCurrentSyncStatus();
        Task<bool> CanAffectItemAsync(string uuid);
    }

    public class ShelterVaultCosmosDBService : IShelterVaultCosmosDBService
    {
        private readonly IVaultManager _vaultManager;
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultCredentials _shelterVaultCredentials;
        private readonly ICloudProviderManager _cloudProviderManager;
        private readonly IShelterVaultSyncStatus _shelterVaultSyncStatus;
        private readonly ILogger<ShelterVaultCosmosDBService> _logger;

        public ShelterVaultCosmosDBService(IVaultManager shelterVaultCreatorManager, IShelterVault shelterVault, ICloudProviderManager cloudProviderManager, IShelterVaultCredentials shelterVaultCredentials, IShelterVaultSyncStatus shelterVaultSyncStatus, ILogger<ShelterVaultCosmosDBService> logger)
        {
            _vaultManager = shelterVaultCreatorManager;
            _shelterVault = shelterVault;
            _cloudProviderManager = cloudProviderManager;
            _shelterVaultCredentials = shelterVaultCredentials;
            _shelterVaultSyncStatus = shelterVaultSyncStatus;
            _logger = logger;
        }

        public async Task UpsertItemAsync<T>(T shelterVault) where T : ICosmosDBModel
        {
            if (!IsSyncEnabled()) return;
            using CosmosClient cosmosClient = GetClient(out CosmosDBSettings cosmosDBSettings);
            Container cosmosContainer = GetContainer(cosmosClient, cosmosDBSettings);
            await cosmosContainer.UpsertItemAsync(item: (object)shelterVault);
        }

        public async Task DeleteItemAsync<T>(T shelterVault) where T : ICosmosDBModel
        {
            if (!IsSyncEnabled()) return;
            using CosmosClient cosmosClient = GetClient(out CosmosDBSettings cosmosDBSettings);
            Container cosmosContainer = GetContainer(cosmosClient, cosmosDBSettings);
            await cosmosContainer.DeleteItemAsync<object>(id: shelterVault.id, partitionKey: new PartitionKey(shelterVault.type));
        }

        public async Task<CosmosDBTinyModel> GetItemByIdAsync(string id)
        {
            if (!IsSyncEnabled()) return null;

            IList<CosmosDBTinyModel> results = await GetCosmosDBItems<CosmosDBTinyModel>(new QueryDefinition("SELECT * FROM vault WHERE vault.id = @id").WithParameter("@id", id));
            return results.FirstOrDefault();
        }

        public async Task SyncAllAsync(string uuidVault)
        {
            try
            {
                if (!IsSyncEnabled(out ShelterVaultSyncStatusModel shelterVaultSyncStatusModel)) return;

                WeakReferenceMessenger.Default.Send(new RefreshCurrentSyncStatusMessage(CloudSyncStatus.SynchInProcess));

                QueryDefinition queryDefinition = GetSyncQueryDefinition(uuidVault, shelterVaultSyncStatusModel);
                IList<CosmosDBTinyModel> cosmosDBTinyModels = await GetCosmosDBItems<CosmosDBTinyModel>(queryDefinition);
                IList<VaultModel> shelterVaults = _vaultManager.GetCurrentVaultWithCredentials();
                IList<CosmosDBSyncModel> cosmosDBSyncModels = CosmosDBTinyModel.ToCosmosDBSyncModel(cosmosDBTinyModels);
                IList<CosmosDBSyncModel> localDBSyncVaults = VaultModel.ToCosmosDBSyncModel(shelterVaults);
                List<CosmosDBSyncModel> syncModels = await cosmosDBSyncModels.SynchronizeVersionsAsync(localDBSyncVaults);

                await MakeUpdates(syncModels, cosmosDBSyncModels);

                WeakReferenceMessenger.Default.Send(new RefreshCredentialListRequestMessage(true));
                WeakReferenceMessenger.Default.Send(new RefreshVaultListRequestMessage(true));

                _shelterVaultSyncStatus.UpdateSyncStatus(CloudProviderType.Azure, CloudSyncStatus.UpToDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing CosmosDB vaults");
                _shelterVaultSyncStatus.UpdateSyncStatus(CloudProviderType.Azure, CloudSyncStatus.SynchFailed);
                throw;
            }
        }

        public CosmosDBSyncStatus GetCurrentSyncStatus()
        {
            ShelterVaultSyncStatusModel shelterVaultSyncStatusModel = _shelterVaultSyncStatus.GetSyncStatus(CloudProviderType.Azure);
            CosmosDBSyncStatus currentSyncStatus = new(shelterVaultSyncStatusModel);
            return currentSyncStatus;
        }

        public QueryDefinition GetSyncQueryDefinition(string uuidVault, ShelterVaultSyncStatusModel shelterVaultSyncStatusModel)
        {
            string uuidVaultClause = $" WHERE ((vault.type = 'shelter_vault' and vault.id='{uuidVault}') or (vault.type = 'shelter_vault_credentials' and vault.shelterVaultUuid = '{uuidVault}'))";
            string timestampClause = shelterVaultSyncStatusModel.Timestamp != 0 ? $" and vault._ts > {shelterVaultSyncStatusModel.Timestamp}" : string.Empty;
            string cosmosDBquery = string.Concat("SELECT vault.id, vault.type, vault.version FROM vault", uuidVaultClause, timestampClause);
            return new QueryDefinition(cosmosDBquery);
        }

        private async Task MakeUpdates(IEnumerable<CosmosDBSyncModel> syncModels, IEnumerable<CosmosDBSyncModel> cosmosDBSyncModels)
        {
            UpsertLocalVaults(await GetCosmosDBItemsByPartitionKey(syncModels, ShelterVaultConstants.PARTITION_SHELTER_VAULT), cosmosDBSyncModels);
            UpsertLocalCredentials(await GetCosmosDBItemsByPartitionKey(syncModels, ShelterVaultConstants.PARTITION_SHELTER_VAULT_CREDENTIALS), cosmosDBSyncModels);
            await UpsertCosmosDbItems(syncModels, ShelterVaultConstants.PARTITION_SHELTER_VAULT);
            await UpsertCosmosDbItems(syncModels, ShelterVaultConstants.PARTITION_SHELTER_VAULT_CREDENTIALS);
            _shelterVaultSyncStatus.UpdateSyncTimestamp(CloudProviderType.Azure, DateTimeOffset.Now.ToUnixTimeSeconds());
            DeleteManyLocal(syncModels, ShelterVaultConstants.PARTITION_SHELTER_VAULT);
            DeleteManyLocal(syncModels, ShelterVaultConstants.PARTITION_SHELTER_VAULT_CREDENTIALS);
        }

        private void DeleteManyLocal(IEnumerable<CosmosDBSyncModel> synchronizedModels, string partitionKey)
        {
            foreach (string id in synchronizedModels.Where(x => x.source == SourceType.Local && x.version == -1 && x.type.Equals(partitionKey)).Select(x => x.id))
            {
                if (partitionKey.Equals(ShelterVaultConstants.PARTITION_SHELTER_VAULT))
                    _shelterVault.DeleteVault(id);
                else if (partitionKey.Equals(ShelterVaultConstants.PARTITION_SHELTER_VAULT_CREDENTIALS))
                    _shelterVaultCredentials.DeleteCredentials(id);
            }
        }

        private void UpsertLocalVaults(IEnumerable<ICosmosDBModel> cosmosDbModels, IEnumerable<CosmosDBSyncModel> cosmosDBSyncModels)
        {
            foreach (var model in cosmosDbModels)
            {
                CosmosDBVault cosmosDBVault = (CosmosDBVault)model;
                CosmosDBSyncModel cosmosDBModel = cosmosDBSyncModels.FirstOrDefault(x => x.id.Equals(cosmosDBVault.id));
                if (cosmosDBModel != null && cosmosDBModel.IsNew) _shelterVault.CreateShelterVault(cosmosDBVault.id, cosmosDBVault.name, cosmosDBVault.encryptedTestValue, cosmosDBVault.iv, cosmosDBVault.salt, cosmosDBVault.version);
                else _shelterVault.UpdateShelterVault(cosmosDBVault.id, cosmosDBVault.name, cosmosDBVault.version);
            }
        }

        private void UpsertLocalCredentials(IEnumerable<ICosmosDBModel> cosmosDbModels, IEnumerable<CosmosDBSyncModel> cosmosDBSyncModels)
        {
            foreach (var model in cosmosDbModels)
            {
                CosmosDBCredentials cosmosDBCredentials = (CosmosDBCredentials)model;
                CosmosDBSyncModel cosmosDBModel = cosmosDBSyncModels.FirstOrDefault(x => x.id.Equals(cosmosDBCredentials.id));
                if (cosmosDBModel != null && cosmosDBModel.IsNew) _shelterVaultCredentials.InsertCredentials(new(cosmosDBCredentials));
                else _shelterVaultCredentials.UpdateCredentials(new(cosmosDBCredentials));
            }
        }

        private async Task UpsertCosmosDbItems(IEnumerable<CosmosDBSyncModel> synchronizedModels, string partitionKey)
        {
            foreach (var id in synchronizedModels.Where(x => x.source == SourceType.Local && x.type == partitionKey).Select(x => x.id))
            {
                IShelterVaultLocalModel shelterVaultLocalModel = partitionKey.Equals(ShelterVaultConstants.PARTITION_SHELTER_VAULT) ? _shelterVault.GetVaultByUUID(id) : _shelterVaultCredentials.GetCredentialsByUUID(id);
                ICosmosDBModel cosmosDBModel = shelterVaultLocalModel.ToCosmosDBModel();
                await UpsertItemAsync(cosmosDBModel);
            }
        }

        private async Task<IEnumerable<ICosmosDBModel>> GetCosmosDBItemsByPartitionKey(IEnumerable<CosmosDBSyncModel> cosmosDBModels, string partitionKey)
        {
            if (!cosmosDBModels.Any()) return Enumerable.Empty<ICosmosDBModel>();

            string baseQuery = string.Concat(partitionKey.Equals(ShelterVaultConstants.PARTITION_SHELTER_VAULT)
                ? "SELECT vault.name, vault.encryptedTestValue, vault.iv, vault.salt, vault.id, vault.type, vault.version FROM vault"
                : "SELECT vault.encryptedValues, vault.iv, vault.shelterVaultUuid, vault.id, vault.type, vault.version FROM vault"
                , " WHERE ARRAY_CONTAINS(@ids, vault.id) AND vault.type = @type");

            QueryDefinition query = new QueryDefinition(baseQuery)
                .WithParameter("@ids", cosmosDBModels.Where(x => x.source == SourceType.CosmosDB && x.type.Equals(partitionKey)).Select(x => x.id))
                .WithParameter("@type", partitionKey);

            return partitionKey.Equals(ShelterVaultConstants.PARTITION_SHELTER_VAULT)
                ? await GetCosmosDBItems<CosmosDBVault>(query)
                : await GetCosmosDBItems<CosmosDBCredentials>(query);
        }

        private async Task<IList<T>> GetCosmosDBItems<T>(QueryDefinition query) where T : ICosmosDBModel
        {
            using CosmosClient cosmosClient = GetClient(out CosmosDBSettings cosmosDBSettings);
            Container cosmosContainer = GetContainer(cosmosClient, cosmosDBSettings);
            using FeedIterator<T> feed = cosmosContainer.GetItemQueryIterator<T>(queryDefinition: query);
            IList<T> results = new List<T>();

            while (feed.HasMoreResults)
            {
                FeedResponse<T> response = await feed.ReadNextAsync();
                foreach (T item in response) results.Add(item);
            }

            return results;
        }

        private CosmosClient GetClient(out CosmosDBSettings cosmosDBSettings)
        {
            cosmosDBSettings = _cloudProviderManager.GetCloudConfiguration<CosmosDBSettings>(CloudProviderType.Azure);
            return new(accountEndpoint: cosmosDBSettings.CosmosEndpoint, authKeyOrResourceToken: cosmosDBSettings.CosmosKey);
        }

        private Container GetContainer(CosmosClient cosmosClient, CosmosDBSettings cosmosDBSettings)
        {
            Database cosmosDb = cosmosClient.GetDatabase(cosmosDBSettings.CosmosDatabase);
            return cosmosDb.GetContainer(cosmosDBSettings.CosmosContainer);
        }

        private bool IsSyncEnabled()
        {
            ShelterVaultSyncStatusModel shelterVaultSyncStatusModel = _shelterVaultSyncStatus.GetSyncStatus(CloudProviderType.Azure);
            return shelterVaultSyncStatusModel.IsSyncEnabled;
        }

        private bool IsSyncEnabled(out ShelterVaultSyncStatusModel shelterVaultSyncStatusModel)
        {
            shelterVaultSyncStatusModel = _shelterVaultSyncStatus.GetSyncStatus(CloudProviderType.Azure);
            return shelterVaultSyncStatusModel.IsSyncEnabled;
        }

        public async Task<bool> CanAffectItemAsync(string uuid)
        {
            CosmosDBTinyModel item = await GetItemByIdAsync(uuid);
            return item != null && item.version > 0;
        }
    }
}
