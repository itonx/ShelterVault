using ShelterVault.Shared.Enums;

namespace ShelterVault.Interfaces
{
    public interface ICloudProviderManager
    {
        bool UpsertCloudConfiguration<T>(
            CloudProviderType cloudProviderType,
            T cloudConfigurationModel
        );
        T GetCloudConfiguration<T>(CloudProviderType cloudProviderType);
        bool UpdateVaultCloudProvider(CloudProviderType cloudProviderType);
        CloudProviderType GetCurrentCloudProvider();
    }
}
