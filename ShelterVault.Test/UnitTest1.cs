using Moq;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;

namespace ShelterVault.Test
{
    public class Tests
    {
        private ShelterVaultCosmosDBService _shelterVaultCosmosDBService;
        private const string VAULT_TYPE = "shelter_vault";
        private const string VAULT_CREDENTIALS_TYPE = "shelter_vault_credentials";

        [SetUp]
        public void Setup()
        {
            var settingsService = new Mock<ISettingsService>();
            var vaultReaderManager = new Mock<IVaultReaderManager>();
            _shelterVaultCosmosDBService = new ShelterVaultCosmosDBService(settingsService.Object, vaultReaderManager.Object);
        }

        [Test]
        public async Task IsSyncModelValidatorOK()
        {
            CosmosDBSyncModel model01 = new CosmosDBSyncModel("abcde01", VAULT_TYPE, 3, SourceType.CosmosDB);
            CosmosDBSyncModel model02 = new CosmosDBSyncModel("abcde02", VAULT_TYPE, 1, SourceType.CosmosDB);
            CosmosDBSyncModel model03 = new CosmosDBSyncModel("abcde03", VAULT_TYPE, 1, SourceType.CosmosDB);
            CosmosDBSyncModel model04 = new CosmosDBSyncModel("abcde04", VAULT_CREDENTIALS_TYPE, 10, SourceType.CosmosDB);
            CosmosDBSyncModel model05 = new CosmosDBSyncModel("abcde05", VAULT_TYPE, 11, SourceType.CosmosDB);
            CosmosDBSyncModel model06 = new CosmosDBSyncModel("cosmosDBItem09", VAULT_TYPE, 1, SourceType.CosmosDB);
            CosmosDBSyncModel model07 = new CosmosDBSyncModel("cosmosDBItem10", VAULT_CREDENTIALS_TYPE, 44, SourceType.CosmosDB);
            List<CosmosDBSyncModel> cosmosDBSyncModels = new List<CosmosDBSyncModel>()
            {
                model01,
                model02,
                model03,
                model04,
                model05,
                model06,
                model07,
            };

            CosmosDBSyncModel localModel01 = new CosmosDBSyncModel("abcde01", VAULT_TYPE, 1, SourceType.Local);
            CosmosDBSyncModel localModel02 = new CosmosDBSyncModel("abcde02", VAULT_TYPE, 10, SourceType.Local);
            CosmosDBSyncModel localModel03 = new CosmosDBSyncModel("abcde03", VAULT_TYPE, 1, SourceType.Local);
            CosmosDBSyncModel localModel04 = new CosmosDBSyncModel("abcde04", VAULT_CREDENTIALS_TYPE, 4, SourceType.Local);
            CosmosDBSyncModel localModel05 = new CosmosDBSyncModel("abcde05", VAULT_TYPE, 1, SourceType.Local);
            CosmosDBSyncModel localModel06 = new CosmosDBSyncModel("localItem01", VAULT_TYPE, 4, SourceType.Local);
            CosmosDBSyncModel localModel07 = new CosmosDBSyncModel("localItem02", VAULT_CREDENTIALS_TYPE, 5, SourceType.Local);
            List<CosmosDBSyncModel> shelterVaultSyncModels = new List<CosmosDBSyncModel>()
            {
                localModel01,
                localModel02,
                localModel03,
                localModel04,
                localModel05,
                localModel06,
                localModel07,
            };
            var results = await _shelterVaultCosmosDBService.GetSynchronizedModelsAsync(cosmosDBSyncModels, shelterVaultSyncModels);
            Assert.That(results.Count, Is.EqualTo(8));
            Assert.That(results.Find(x => x.id.Equals(model01.id)), Is.EqualTo(model01));
            Assert.That(results.Find(x => x.id.Equals(localModel02.id)), Is.EqualTo(localModel02));
            Assert.That(results.Find(x => x.id.Equals(model04.id)), Is.EqualTo(model04));
            Assert.That(results.Find(x => x.id.Equals(model05.id)), Is.EqualTo(model05));
            Assert.That(results.Find(x => x.id.Equals(model06.id)), Is.EqualTo(model06));
            Assert.That(results.Find(x => x.id.Equals(model07.id)), Is.EqualTo(model07));
            Assert.That(results.Find(x => x.id.Equals(localModel06.id)), Is.EqualTo(localModel06));
            Assert.That(results.Find(x => x.id.Equals(localModel07.id)), Is.EqualTo(localModel07));
        }
    }
}