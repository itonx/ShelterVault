using System;
using ShelterVault.DataLayer;
using ShelterVault.Interfaces;
using ShelterVault.Models;
using ShelterVault.Shared.Enums;

namespace ShelterVault.Managers
{
    public class CloudProviderManager : ICloudProviderManager
    {
        private readonly IShelterVaultEncryption _encryptionService;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly IShelterVaultCloudConfig _shelterVaultCloudConfig;
        private readonly IShelterVault _shelterVault;

        public CloudProviderManager(
            IShelterVaultEncryption encryptionService,
            IShelterVaultStateService shelterVaultStateService,
            IShelterVaultCloudConfig shelterVaultCloudConfig,
            IShelterVault shelterVault
        )
        {
            _encryptionService = encryptionService;
            _shelterVaultStateService = shelterVaultStateService;
            _shelterVaultCloudConfig = shelterVaultCloudConfig;
            _shelterVault = shelterVault;
        }

        public T GetCloudConfiguration<T>(CloudProviderType cloudProviderType)
        {
            ShelterVaultCloudConfigModel shelterVaultCloudConfigModel =
                _shelterVaultCloudConfig.GetCloudConfiguration(cloudProviderType.ToString());
            if (shelterVaultCloudConfigModel == null)
                return default(T);
            (byte[] encryptionKey, byte[] salt) =
                _shelterVaultStateService.GetLocalEncryptionValues();
            string decryptedJsonModel = _encryptionService.DecryptAes(
                shelterVaultCloudConfigModel,
                encryptionKey
            );
            return System.Text.Json.JsonSerializer.Deserialize<T>(decryptedJsonModel);
        }

        public bool UpsertCloudConfiguration<T>(
            CloudProviderType cloudProviderType,
            T cloudConfigurationModel
        )
        {
            try
            {
                string jsonModel = System.Text.Json.JsonSerializer.Serialize(
                    cloudConfigurationModel
                );
                (byte[] encryptionKey, _) = _shelterVaultStateService.GetLocalEncryptionValues();
                (byte[], byte[]) encryptedValues = _encryptionService.EncryptAes(
                    jsonModel,
                    encryptionKey
                );

                ShelterVaultCloudConfigModel config = new(
                    cloudProviderType.ToString(),
                    encryptedValues
                );
                bool result = _shelterVaultCloudConfig.UpsertCloudConfiguration(
                    config.Name,
                    config.EncryptedValues,
                    config.Iv
                );

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
