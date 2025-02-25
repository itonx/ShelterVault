using ShelterVault.Models;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Extensions;

namespace ShelterVault.Test
{
    public class Tests
    {
        private const string VAULT_TYPE = ShelterVaultConstants.PARTITION_SHELTER_VAULT;
        private const string VAULT_CREDENTIALS_TYPE = ShelterVaultConstants.PARTITION_SHELTER_VAULT_CREDENTIALS;

        [Test]
        public async Task IsSynchronizationOK()
        {
            CosmosDBSyncModel model01 = new CosmosDBSyncModel("abcde01", VAULT_TYPE, 3, SourceType.CosmosDB);
            CosmosDBSyncModel model02 = new CosmosDBSyncModel("abcde02", VAULT_TYPE, 1, SourceType.CosmosDB);
            CosmosDBSyncModel model03 = new CosmosDBSyncModel("abcde03", VAULT_TYPE, 1, SourceType.CosmosDB);
            CosmosDBSyncModel model04 = new CosmosDBSyncModel("abcde04", VAULT_CREDENTIALS_TYPE, 10, SourceType.CosmosDB);
            CosmosDBSyncModel model05 = new CosmosDBSyncModel("abcde05", VAULT_TYPE, 11, SourceType.CosmosDB);
            CosmosDBSyncModel model06 = new CosmosDBSyncModel("cosmosDBItem09", VAULT_TYPE, 1, SourceType.CosmosDB);
            CosmosDBSyncModel model07 = new CosmosDBSyncModel("cosmosDBItem10", VAULT_CREDENTIALS_TYPE, 44, SourceType.CosmosDB);
            CosmosDBSyncModel model08 = new CosmosDBSyncModel("deleted01", VAULT_CREDENTIALS_TYPE, -1, SourceType.CosmosDB);
            CosmosDBSyncModel model09 = new CosmosDBSyncModel("deleted02", VAULT_CREDENTIALS_TYPE, 3, SourceType.CosmosDB);
            CosmosDBSyncModel model10 = new CosmosDBSyncModel("deleted03", VAULT_CREDENTIALS_TYPE, -1, SourceType.CosmosDB);
            List<CosmosDBSyncModel> cosmosDBSyncModels = new List<CosmosDBSyncModel>()
            {
                model01,
                model02,
                model03,
                model04,
                model05,
                model06,
                model07,
                model08,
                model09,
                model10,
            };

            CosmosDBSyncModel localModel01 = new CosmosDBSyncModel("abcde01", VAULT_TYPE, 1, SourceType.Local);
            CosmosDBSyncModel localModel02 = new CosmosDBSyncModel("abcde02", VAULT_TYPE, 10, SourceType.Local);
            CosmosDBSyncModel localModel03 = new CosmosDBSyncModel("abcde03", VAULT_TYPE, 1, SourceType.Local);
            CosmosDBSyncModel localModel04 = new CosmosDBSyncModel("abcde04", VAULT_CREDENTIALS_TYPE, 4, SourceType.Local);
            CosmosDBSyncModel localModel05 = new CosmosDBSyncModel("abcde05", VAULT_TYPE, 1, SourceType.Local);
            CosmosDBSyncModel localModel06 = new CosmosDBSyncModel("localItem01", VAULT_TYPE, 4, SourceType.Local);
            CosmosDBSyncModel localModel07 = new CosmosDBSyncModel("localItem02", VAULT_CREDENTIALS_TYPE, 5, SourceType.Local);
            CosmosDBSyncModel localModel08 = new CosmosDBSyncModel("deleted01", VAULT_CREDENTIALS_TYPE, 3, SourceType.Local);
            CosmosDBSyncModel localModel09 = new CosmosDBSyncModel("deleted02", VAULT_CREDENTIALS_TYPE, -1, SourceType.Local);
            CosmosDBSyncModel localModel10 = new CosmosDBSyncModel("deleted03", VAULT_CREDENTIALS_TYPE, -1, SourceType.Local);
            List<CosmosDBSyncModel> shelterVaultSyncModels = new List<CosmosDBSyncModel>()
            {
                localModel01,
                localModel02,
                localModel03,
                localModel04,
                localModel05,
                localModel06,
                localModel07,
                localModel08,
                localModel09,
                localModel10,
            };
            var results = await cosmosDBSyncModels.SynchronizeVersionsAsync(shelterVaultSyncModels);
            Assert.That(results.Count, Is.EqualTo(10));
            Assert.That(results.Find(x => x.id.Equals(model01.id)), Is.EqualTo(model01));
            Assert.That(results.Find(x => x.id.Equals(localModel02.id)), Is.EqualTo(localModel02));
            Assert.That(results.Find(x => x.id.Equals(model04.id)), Is.EqualTo(model04));
            Assert.That(results.Find(x => x.id.Equals(model05.id)), Is.EqualTo(model05));
            Assert.That(results.Find(x => x.id.Equals(model06.id)), Is.EqualTo(model06));
            Assert.That(results.Find(x => x.id.Equals(model07.id)), Is.EqualTo(model07));
            Assert.That(results.Find(x => x.id.Equals(localModel06.id)), Is.EqualTo(localModel06));
            Assert.That(results.Find(x => x.id.Equals(localModel07.id)), Is.EqualTo(localModel07));
            Assert.That(results.Find(x => x.id.Equals(model08.id)), Is.EqualTo(model08));
            Assert.That(results.Find(x => x.id.Equals(localModel09.id)), Is.EqualTo(localModel09));
        }
    }
}