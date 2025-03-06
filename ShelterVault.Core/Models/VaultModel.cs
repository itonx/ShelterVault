using System.Collections.Generic;

namespace ShelterVault.Models
{
    public class VaultModel
    {
        public ShelterVaultModel ShelterVault { get; set; }
        public IEnumerable<ShelterVaultCredentialsModel> ShelterVaultCredentials { get; set; }

        public VaultModel(ShelterVaultModel shelterVault, IEnumerable<ShelterVaultCredentialsModel> shelterVaultCredentials)
        {
            ShelterVault = shelterVault;
            ShelterVaultCredentials = shelterVaultCredentials;
        }

        public static IList<CosmosDBSyncModel> ToCosmosDBSyncModel(IList<VaultModel> vaultModels)
        {
            IList<CosmosDBSyncModel> cosmosDBSyncModels = new List<CosmosDBSyncModel>();
            foreach (VaultModel vaultModel in vaultModels)
            {
                cosmosDBSyncModels.Add(new CosmosDBSyncModel(vaultModel.ShelterVault.UUID, "shelter_vault", vaultModel.ShelterVault.Version, SourceType.Local));
                foreach (ShelterVaultCredentialsModel shelterVaultCredentialsModel in vaultModel.ShelterVaultCredentials)
                {
                    cosmosDBSyncModels.Add(new CosmosDBSyncModel(shelterVaultCredentialsModel.UUID, "shelter_vault_credentials", shelterVaultCredentialsModel.Version, SourceType.Local));
                }
            }
            return cosmosDBSyncModels;
        }
    }
}