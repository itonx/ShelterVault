using Dapper;
using Microsoft.Data.Sqlite;
using ShelterVault.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.DataLayer
{
    public interface IShelterVaultLocalStorageStorage
    {
        bool CreateVault(string uuid, string name, string masterKey, string iv, string salt, long version);
        bool IsMasterKeyValid(string masterKey);
        bool UpsertCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel);
        bool DeleteCredentials(string uuid);
        IEnumerable<ShelterVaultCredentialsModel> GetCredentials(bool active = true);
        ShelterVaultCredentialsModel GetCredentialsByUUID(string uuid);
        bool UpsertCloudConfiguration(string name, string encryptedValues, string iv);
        ShelterVaultCloudConfigModel GetCloudConfiguration(string name);
        IEnumerable<ShelterVaultModel> GetShelterVaults();
        ShelterVaultModel GetVaultByUUID(string uuid);
    }

    public class ShelterVaultCredentialsLocalStorage : SQLite3LocalStorage, IShelterVaultLocalStorageStorage
    {
        private readonly string[] shelterVaultModelQueries = new string[]
        {
            @"
            CREATE TABLE IF NOT EXISTS shelter_vault (
                uuid TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                masterKeyHash TEXT NOT NULL,
                iv TEXT NOT NULL,
                salt TEXT NOT NULL,
                version INTEGER NOT NULL,
                UNIQUE(name)
            )",

            @"
            CREATE TABLE IF NOT EXISTS shelter_vault_credentials (
                uuid TEXT PRIMARY KEY,
                encryptedValues TEXT NOT NULL,
                iv TEXT NOT NULL,
                shelterVaultUuid TEXT NOT NULL,
                version INTEGER NOT NULL,
                FOREIGN KEY(shelterVaultUuid) REFERENCES shelter_vault(uuid)
            )",
            @"
            CREATE TABLE IF NOT EXISTS shelter_vault_cloud_config (
                name TEXT PRIMARY KEY,
                encryptedValues TEXT NOT NULL,
                iv TEXT NOT NULL
            )",

            @"
            INSERT INTO shelter_vault
            VALUES ($uuid, $name, $masterKeyHash, $iv, $salt, $version)
            "
        };

        public bool CreateVault(string uuid, string name, string masterKey, string iv, string salt, long version)
        {
            try
            {
                using (var connection = new SqliteConnection(DbConnectionString))
                {
                    foreach (var query in shelterVaultModelQueries)
                    {
                        connection.Query(query);
                    }
                    string insertVaultQuery = @"
                    INSERT INTO shelter_vault
                    VALUES ($uuid, $name, $masterKeyHash, $iv, $salt, $version)
                    ";
                    connection.Query(insertVaultQuery, new { uuid, name, masterKeyHash = masterKey, iv, salt, version });
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteCredentials(string uuid)
        {
            throw new NotImplementedException();
        }

        public ShelterVaultCloudConfigModel GetCloudConfiguration(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ShelterVaultCredentialsModel> GetCredentials(bool active = true)
        {
            return Enumerable.Empty<ShelterVaultCredentialsModel>();
        }

        public ShelterVaultCredentialsModel GetCredentialsByUUID(string uuid)
        {
            return null;
        }

        public IEnumerable<ShelterVaultModel> GetShelterVaults()
        {
            throw new NotImplementedException();
        }

        public ShelterVaultModel GetVaultByUUID(string uuid)
        {
            throw new NotImplementedException();
        }

        public bool IsMasterKeyValid(string masterKey)
        {
            throw new NotImplementedException();
        }

        public bool UpsertCloudConfiguration(string name, string encryptedValues, string iv)
        {
            throw new NotImplementedException();
        }

        public bool UpsertCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel)
        {
            return true;
        }
    }
}
