using System.Threading.Tasks;
using ShelterVault.Models;

namespace ShelterVault.Interfaces
{
    public interface ICloudSyncManager
    {
        Task<bool> UpsertItemAsync<T>(T shelterVaultModel, bool validateItem = false)
            where T : IShelterVaultLocalModel;
        Task DeleteItemAsync<T>(T shelterVaultModel)
            where T : IShelterVaultLocalModel;
        Task<ICosmosDBModel> GetItemAsync<T>(T shelterVaultModel)
            where T : IShelterVaultLocalModel;
        Task SyncVaults();
        CloudSyncInformation GetCurrentCloudSyncInformation();
    }
}
