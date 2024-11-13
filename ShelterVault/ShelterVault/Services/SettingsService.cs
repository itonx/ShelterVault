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
    interface ISettingsService
    {
        CloudProviderType GetCurrentCloudProviderType();
        void SaveCloudProviderType(CloudProviderType cloudProviderType);
    }

    class SettingsService : ISettingsService
    {
        ApplicationDataContainer _localSettings;

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
    }
}
