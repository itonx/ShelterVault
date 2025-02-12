using Microsoft.Azure.Cosmos;
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
        Task SyncAll();
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

        public async Task SyncAll()
        {
            CosmosDBSettings cosmosDBSettings = _settingsService.ReadJsonValueAs<CosmosDBSettings>(ShelterVaultConstants.COSMOS_DB_SETTINGS);
            IList<VaultModel> vaults = _vaultReaderManager.GetAllVaults();
            using CosmosClient cosmosClient = new(accountEndpoint: cosmosDBSettings.CosmosEndpoint, authKeyOrResourceToken: cosmosDBSettings.CosmosKey);
            Database cosmosDb = cosmosClient.GetDatabase(cosmosDBSettings.CosmosDatabase);
            Container cosmosContainer = cosmosDb.GetContainer(cosmosDBSettings.CosmosContainer);

            foreach (VaultModel vault in vaults)
            {
                CosmosDBVault cosmosDBVault = vault.ShelterVault.ToCosmosDBVault();
                await cosmosContainer.UpsertItemAsync(item: cosmosDBVault, partitionKey: new PartitionKey("shelter_vault"));
                foreach (var credential in vault.ShelterVaultCredentials)
                {
                    await cosmosContainer.UpsertItemAsync(item: credential.ToCosmosDBCredentials(), partitionKey: new PartitionKey("shelter_vault_credentials"));
                }
            }
        }
    }
}
