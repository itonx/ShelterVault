using ShelterVault.Models;

namespace ShelterVault.DataLayer
{
    public interface IShelterVaultCloudConfig
    {
        ShelterVaultCloudConfigModel GetCloudConfiguration(string name);
        bool UpsertCloudConfiguration(string name, string encryptedValues, string iv);
    }

    public class ShelterVaultCloudConfig : IShelterVaultCloudConfig
    {
        private readonly IShelterVaultLocalDb _shelterVaultLocalDb;

        public ShelterVaultCloudConfig(IShelterVaultLocalDb shelterVaultLocalDb)
        {
            _shelterVaultLocalDb = shelterVaultLocalDb;
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
    }




}