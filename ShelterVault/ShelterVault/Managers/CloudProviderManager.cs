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
    }
}
