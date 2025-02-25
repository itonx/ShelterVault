using ShelterVault.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Extensions
{
    public static class SyncExtensions
    {
        public static async Task<List<CosmosDBSyncModel>> SynchronizeVersionsAsync(this IList<CosmosDBSyncModel> cosmosDBSyncModels, IList<CosmosDBSyncModel> shelterVaultSyncModels)
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
                        else if (cosmosDBItem.Value.version == -1)
                            syncModels.Add(cosmosDBItem.Value);
                        else if (shelterVaultItems[cosmosDBItem.Key].version == -1)
                            syncModels.Add(shelterVaultItems[cosmosDBItem.Key]);
                        else if (shelterVaultItems[cosmosDBItem.Key].version < cosmosDBItem.Value.version)
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
    }
}
