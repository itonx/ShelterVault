using ShelterVault.Models;
using ShelterVault.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ShelterVault.DataLayer
{
    public interface IShelterVault
    {
        bool CreateShelterVault(string uuid, string name, string encryptedTestValue, string iv, string salt, long version);
        bool UpdateShelterVault(string uuid, string name, long version);
        bool UpdateVaultCloudProvider(int cloudProvider);
        bool DeleteVault(string uuid);
        IEnumerable<ShelterVaultModel> GetAllActiveVaults();
        ShelterVaultModel GetVaultByUUID(string uuid);
        ShelterVaultModel GetCurrentVault();
        bool AreThereVaults();
    }

    public class ShelterVault : IShelterVault
    {
        private readonly IShelterVaultLocalDb _shelterVaultLocalDb;
        private readonly IShelterVaultCredentials _shelterVaultCredentials;

        public ShelterVault(IShelterVaultLocalDb shelterVaultLocalDb, IShelterVaultCredentials shelterVaultCredentials)
        {
            _shelterVaultLocalDb = shelterVaultLocalDb;
            _shelterVaultCredentials = shelterVaultCredentials;
        }

        public bool AreThereVaults() => GetAllActiveVaults().Any();

        public bool CreateShelterVault(string uuid, string name, string encryptedTestValue, string iv, string salt, long version)
        {
            KeyValuePair<string, object> insertVault = new("INSERT INTO shelter_vault VALUES ($uuid, $name, $encryptedTestValue, $iv, $salt, $version, $cloudProvider)", new { uuid, name, encryptedTestValue, iv, salt, version, cloudProvider = 0 });
            var queries = new List<KeyValuePair<string, object>>(ShelterVaultQueries.CREATE_SHELTER_VAULT_DB) { insertVault };

            return _shelterVaultLocalDb.ExecuteQueries(queries);
        }

        public bool UpdateVaultCloudProvider(int cloudProvider)
        {
            string query = @"
                UPDATE shelter_vault
                SET cloudProvider=$cloudProvider
            ";
            int updatedVaults = _shelterVaultLocalDb.Execute(query, new { cloudProvider });
            return updatedVaults > 0;
        }

        public bool UpdateShelterVault(string uuid, string name, long version)
        {

            string updateVaultQuery = @"
                UPDATE shelter_vault SET name=$name, version=$version
                WHERE uuid=$uuid
            ";
            object param = new { uuid, name, version };
            int updatedCredentials = _shelterVaultLocalDb.Execute(updateVaultQuery, param);
            return updatedCredentials > 0;
        }

        public bool DeleteVault(string uuid)
        {
            string query = @"
                DELETE FROM shelter_vault
                WHERE uuid=$uuid
            ";
            int vaultsDeleted = _shelterVaultLocalDb.Execute(query, new { uuid });

            return _shelterVaultCredentials.DeleteCredentialsByVaultId(uuid) || vaultsDeleted > 0;
        }

        public IEnumerable<ShelterVaultModel> GetAllActiveVaults()
        {
            IEnumerable<string> fileNames = _shelterVaultLocalDb.DefaultShelterVaultPath.GetFileNamesByExtension(_shelterVaultLocalDb.DbExtension);
            if (!fileNames.Any()) return Enumerable.Empty<ShelterVaultModel>();
            string query = "SELECT * FROM shelter_vault WHERE version > 0";
            IEnumerable<ShelterVaultModel> vaults = _shelterVaultLocalDb.QueryAcrossDatabases<ShelterVaultModel>(fileNames, query);
            return vaults;
        }

        public ShelterVaultModel GetCurrentVault()
        {
            ShelterVaultModel result = _shelterVaultLocalDb.QueryFirst<ShelterVaultModel>("SELECT * FROM shelter_vault");
            return result;
        }

        public ShelterVaultModel GetVaultByUUID(string uuid)
        {
            ShelterVaultModel result = _shelterVaultLocalDb.QueryFirst<ShelterVaultModel>("SELECT * FROM shelter_vault WHERE uuid=$uuid", new { uuid });
            return result;
        }

        public bool UpsertCloudConfiguration(string name, string encryptedValues, string iv)
        {
            ShelterVaultCloudConfigModel model = GetCloudConfiguration(name);
            string query = string.Empty;
            if (model == null)
            {
                query = @"
                    INSERT INTO shelter_vault_cloud_config
                    VALUES($name, $encryptedValues, $iv)
                ";
            }
            else
            {
                query = @"
                    UPDATE shelter_vault_cloud_config
                    SET
                    encryptedValues=$encryptedValues, iv=$iv
                    WHERE name=$name
                ";
            }
            int result = _shelterVaultLocalDb.Execute(query, new { encryptedValues, name, iv });
            return result == 1;
        }

        public ShelterVaultCloudConfigModel GetCloudConfiguration(string name)
        {
            string query = @"
                SELECT * FROM shelter_vault_cloud_config
                WHERE name=$name
            ";

            ShelterVaultCloudConfigModel result = _shelterVaultLocalDb.QueryFirstOrDefault<ShelterVaultCloudConfigModel>(query, new { name });
            return result;
        }
    }
}