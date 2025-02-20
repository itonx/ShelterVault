using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ShelterVault.Services
{
    public interface ISettingsService
    {
        CloudProviderType GetCurrentCloudProviderType();
        void SaveCloudProviderType(CloudProviderType cloudProviderType);
        void SaveAsJsonValue(string key, object settingObj);
        T ReadJsonValueAs<T>(string key);
    }

    public class SettingsService : ISettingsService
    {
        readonly ApplicationDataContainer _localSettings;

        public SettingsService()
        {
            _localSettings = ApplicationData.Current.LocalSettings;
        }

        public CloudProviderType GetCurrentCloudProviderType()
        {
            string cloudProviderTypeStr = _localSettings.Values[ShelterVaultConstants.SETTINGS_CLOUD_PROVIDER] as string;
            Enum.TryParse(typeof(CloudProviderType), cloudProviderTypeStr, true, out object cloudProviderTypeObj);
            
            return cloudProviderTypeObj == null ? CloudProviderType.None : (CloudProviderType)cloudProviderTypeObj;
        }

        public void SaveCloudProviderType(CloudProviderType cloudProviderType)
        {
            _localSettings.Values[ShelterVaultConstants.SETTINGS_CLOUD_PROVIDER] = cloudProviderType.ToString();
        }

        public void SaveAsJsonValue(string key, object settingObj)
        {
            _localSettings.Values[key] = System.Text.Json.JsonSerializer.Serialize(settingObj);
        }

        public T ReadJsonValueAs<T>(string key)
        {
            string value = _localSettings.Values[key] as string;
            if (string.IsNullOrWhiteSpace(value)) return default(T);
            return System.Text.Json.JsonSerializer.Deserialize<T>(value);
        }
    }
}
