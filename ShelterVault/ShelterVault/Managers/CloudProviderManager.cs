using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Enums;
using System;

namespace ShelterVault.Managers
{
    public interface ICloudProviderManager
    {
        bool UpsertCloudConfiguration<T>(CloudProviderType cloudProviderType, T cloudConfigurationModel);
        T GetCloudConfiguration<T>(CloudProviderType cloudProviderType);
        bool UpdateVaultCloudProvider(CloudProviderType cloudProviderType);
        CloudProviderType GetCurrentCloudProvider();
    }

    public class CloudProviderManager : ICloudProviderManager
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly IShelterVaultCloudConfig _shelterVaultCloudConfig;
        private readonly IShelterVault _shelterVault;

        public CloudProviderManager(IEncryptionService encryptionService, IShelterVaultStateService shelterVaultStateService, IShelterVaultCloudConfig shelterVaultCloudConfig, IShelterVault shelterVault)
        {
            _encryptionService = encryptionService;
            _shelterVaultStateService = shelterVaultStateService;
            _shelterVaultCloudConfig = shelterVaultCloudConfig;
            _shelterVault = shelterVault;
        }

        public T GetCloudConfiguration<T>(CloudProviderType cloudProviderType)
        {
            ShelterVaultCloudConfigModel shelterVaultCloudConfigModel = _shelterVaultCloudConfig.GetCloudConfiguration(cloudProviderType.ToString());
            if (shelterVaultCloudConfigModel == null) return default(T);
            (byte[] derivedKey, byte[] salt) = _shelterVaultStateService.GetLocalEncryptionValues();
            string decryptedJsonModel = _encryptionService.DecryptAes(shelterVaultCloudConfigModel, derivedKey, salt);
            return System.Text.Json.JsonSerializer.Deserialize<T>(decryptedJsonModel);
        }

        public bool UpsertCloudConfiguration<T>(CloudProviderType cloudProviderType, T cloudConfigurationModel)
        {
            try
            {
                string jsonModel = System.Text.Json.JsonSerializer.Serialize(cloudConfigurationModel);
                (byte[] derivedKey, byte[] salt) = _shelterVaultStateService.GetLocalEncryptionValues();
                (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(jsonModel, derivedKey, salt);

                ShelterVaultCloudConfigModel config = new(cloudProviderType.ToString(), encryptedValues);
                bool result = _shelterVaultCloudConfig.UpsertCloudConfiguration(config.Name, config.EncryptedValues, config.Iv);

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateVaultCloudProvider(CloudProviderType cloudProviderType)
        {
            return _shelterVault.UpdateVaultCloudProvider((int)cloudProviderType);
        }

        public CloudProviderType GetCurrentCloudProvider()
        {
            return (CloudProviderType)(_shelterVault.GetCurrentVault()?.CloudProvider ?? 0);
        }
    }
}
