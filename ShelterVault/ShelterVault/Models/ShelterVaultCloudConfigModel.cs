using ShelterVault.Shared.Extensions;

namespace ShelterVault.Models
{
    public class ShelterVaultCloudConfigModel
    {
        public string Name { get; set; }
        public string EncryptedValues { get; set; }
        public string Iv { get; set; }

        public ShelterVaultCloudConfigModel()
        {
            Name = string.Empty;
            EncryptedValues = string.Empty;
            Iv = string.Empty;
        }

        public ShelterVaultCloudConfigModel(string name, (byte[], byte[]) encyptedValues)
        {
            Name = name;
            EncryptedValues = encyptedValues.Item1.ToBase64();
            Iv = encyptedValues.Item2.ToBase64();
        }
    }
}