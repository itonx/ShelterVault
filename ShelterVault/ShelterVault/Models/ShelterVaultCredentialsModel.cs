using ShelterVault.Shared.Extensions;
using System;

namespace ShelterVault.Models
{
    public class ShelterVaultCredentialsModel : IShelterVaultLocalModel
    {
        public string UUID { get; set; }
        public string EncryptedValues { get; set; }
        public string Iv { get; set; }
        public string ShelterVaultUuid { get; set; }
        public long Version { get; set; }

        public ShelterVaultCredentialsModel()
        {
            UUID = string.Empty;
            EncryptedValues = string.Empty;
            Iv = string.Empty;
            ShelterVaultUuid = string.Empty;
            Version = 0;
        }

        public ShelterVaultCredentialsModel(string shelterVaultUuid, (byte[], byte[]) encyptedValues)
        {
            UUID = Guid.NewGuid().ToString();
            EncryptedValues = encyptedValues.Item1.ToBase64();
            Iv = encyptedValues.Item2.ToBase64();
            ShelterVaultUuid = shelterVaultUuid;
            Version = 1;
        }

        public ShelterVaultCredentialsModel(Credentials credentials, (byte[], byte[]) encyptedValues)
        {
            UUID = credentials.UUID;
            EncryptedValues = encyptedValues.Item1.ToBase64();
            Iv = encyptedValues.Item2.ToBase64();
            ShelterVaultUuid = credentials.ShelterVaultUuid;
            Version = credentials.Version + 1;
        }

        public ShelterVaultCredentialsModel(ShelterVaultCredentialsModel shelterVaultCredentialsModel)
        {
            UUID = shelterVaultCredentialsModel.UUID;
            EncryptedValues = shelterVaultCredentialsModel.EncryptedValues;
            Iv = shelterVaultCredentialsModel.Iv;
            ShelterVaultUuid = shelterVaultCredentialsModel.ShelterVaultUuid;
            Version = shelterVaultCredentialsModel.Version;
        }

        public ShelterVaultCredentialsModel(CosmosDBCredentials cosmosDBCredentials)
        {
            UUID = cosmosDBCredentials.id;
            EncryptedValues = cosmosDBCredentials.encryptedValues;
            Iv = cosmosDBCredentials.iv;
            ShelterVaultUuid = cosmosDBCredentials.shelterVaultUuid;
            Version = cosmosDBCredentials.version;
        }

        public ICosmosDBModel ToCosmosDBModel()
        {
            CosmosDBCredentials credentials = new(UUID, EncryptedValues, Iv, ShelterVaultUuid, Version);
            return credentials;
        }

        public void MarkAsDeleted()
        {
            EncryptedValues = string.Empty;
            Iv = string.Empty;
            Version = -1;
        }
    }
}