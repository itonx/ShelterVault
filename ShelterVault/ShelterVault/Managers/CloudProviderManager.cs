using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Managers
{
    public interface ICloudProviderManager
    {
        bool UpsertCloudConfiguration<T>(CloudProviderType cloudProviderType, T cloudConfigurationModel);
        T GetCloudConfiguration<T>(CloudProviderType cloudProviderType);
        ShelterVaultSyncStatusModel GetSyncStatus(CloudProviderType cloudProviderType);
        bool DisableSync(CloudProviderType cloudProviderType);
        bool UpdateSyncTimestamp(CloudProviderType cloudProviderType, long timestamp);
        bool UpdateSyncStatus(CloudProviderType cloudProviderType, CloudSyncStatus cloudSyncStatus);
        bool UpsertSyncStatus(CloudProviderType cloudProviderType, long timestamp, bool isSyncEnabled, CloudSyncStatus cloudSyncStatus);
        bool UpdateVaultCloudProvider(CloudProviderType cloudProviderType);
        CloudProviderType GetCurrentCloudProvider();
    }

    public class CloudProviderManager : ICloudProviderManager
    {
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IEncryptionService _encryptionService;
        private readonly IShelterVaultStateService _shelterVaultStateService;

        public CloudProviderManager(IShelterVaultLocalStorage shelterVaultLocalStorage, IEncryptionService encryptionService, IShelterVaultStateService shelterVaultStateService)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _encryptionService = encryptionService;
            _shelterVaultStateService = shelterVaultStateService;
        }

        public T GetCloudConfiguration<T>(CloudProviderType cloudProviderType)
        {
            ShelterVaultCloudConfigModel shelterVaultCloudConfigModel = _shelterVaultLocalStorage.GetCloudConfiguration(cloudProviderType.ToString());
            if (shelterVaultCloudConfigModel == null) return default(T);
            byte[] masterKey = _shelterVaultStateService.GetMasterKeyUnprotected();
            byte[] salt = _shelterVaultStateService.GetMasterKeySaltUnprotected();
            string decryptedJsonModel = _encryptionService.DecryptAes(shelterVaultCloudConfigModel, masterKey, salt);
            return System.Text.Json.JsonSerializer.Deserialize<T>(decryptedJsonModel);
        }

        public ShelterVaultSyncStatusModel GetSyncStatus(CloudProviderType cloudProviderType)
        {
            return _shelterVaultLocalStorage.GetSyncStatus(cloudProviderType.ToString());
        }

        public bool UpsertSyncStatus(CloudProviderType cloudProviderType, long timestamp, bool isSyncEnabled, CloudSyncStatus cloudSyncStatus)
        {
            return _shelterVaultLocalStorage.UpsertSyncStatus(cloudProviderType.ToString(), timestamp, isSyncEnabled, cloudSyncStatus);
        }

        public bool DisableSync(CloudProviderType cloudProviderType)
        {
            return UpsertSyncStatus(cloudProviderType, 0, false, CloudSyncStatus.None);
        }

        public bool UpdateSyncTimestamp(CloudProviderType cloudProviderType, long timestamp)
        {
            return _shelterVaultLocalStorage.UpdateSyncTimestamp(cloudProviderType.ToString(), timestamp);
        }

        public bool UpdateSyncStatus(CloudProviderType cloudProviderType, CloudSyncStatus cloudSyncStatus)
        {
            return _shelterVaultLocalStorage.UpdateSyncStatus(cloudProviderType.ToString(), cloudSyncStatus);
        }

        public bool UpsertCloudConfiguration<T>(CloudProviderType cloudProviderType, T cloudConfigurationModel)
        {
            try
            {
                string jsonModel = System.Text.Json.JsonSerializer.Serialize(cloudConfigurationModel);
                byte[] masterKey = _shelterVaultStateService.GetMasterKeyUnprotected();
                byte[] salt = _shelterVaultStateService.GetMasterKeySaltUnprotected();

                (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(jsonModel, masterKey, salt);

                ShelterVaultCloudConfigModel config = new(cloudProviderType.ToString(), encryptedValues);
                bool result = _shelterVaultLocalStorage.UpsertCloudConfiguration(config.Name, config.EncryptedValues, config.Iv);

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateVaultCloudProvider(CloudProviderType cloudProviderType)
        {
            return _shelterVaultLocalStorage.UpdateVaultCloudProvider((int)cloudProviderType);
        }

        public CloudProviderType GetCurrentCloudProvider()
        {
            return (CloudProviderType)(_shelterVaultLocalStorage.GetCurrentVault()?.CloudProvider ?? 0);
        }
    }
}
