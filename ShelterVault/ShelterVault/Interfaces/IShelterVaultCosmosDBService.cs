using System.Threading.Tasks;
using ShelterVault.Models;

namespace ShelterVault.Interfaces
{
    public interface IShelterVaultCosmosDBService
    {
        Task UpsertItemAsync<T>(T shelterVault)
            where T : ICosmosDBModel;
        Task DeleteItemAsync<T>(T shelterVault)
            where T : ICosmosDBModel;
        Task SyncAllAsync(string uuidVault);
        Task<CosmosDBTinyModel> GetItemByIdAsync(string id);
        CosmosDBSyncStatus GetCurrentSyncStatus();
        Task<bool> CanAffectItemAsync(string uuid);
    }
}
