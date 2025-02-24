using ShelterVault.DataLayer;
using ShelterVault.Managers;
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
        void SaveAsJsonValue(string key, object settingObj);
        T ReadJsonValueAs<T>(string key);
    }

    public class SettingsService : ISettingsService
    {
        readonly ApplicationDataContainer _localSettings;

        public SettingsService(ICloudProviderManager cloudProviderManager)
        {
            _localSettings = ApplicationData.Current.LocalSettings;
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
