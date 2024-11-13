using CommunityToolkit.Mvvm.ComponentModel;
using ShelterVault.Services;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private IList<CloudProviderType> _cloudProviders;
        [ObservableProperty]
        private CloudProviderType _selectedCloudProvider;

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            CloudProviders = new List<CloudProviderType>((CloudProviderType[])Enum.GetValues(typeof(CloudProviderType)));
            SelectedCloudProvider = _settingsService.GetCurrentCloudProviderType();
        }

        partial void OnSelectedCloudProviderChanged(CloudProviderType value)
        {
            _settingsService.SaveCloudProviderType(value);
        }
    }
}
