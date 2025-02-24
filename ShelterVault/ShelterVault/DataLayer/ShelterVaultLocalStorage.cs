using Dapper;
using Microsoft.Data.Sqlite;
using ShelterVault.Models;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.DataLayer
{
    public interface IShelterVaultLocalStorage
    {
        bool DBExists();
        bool CreateShelterVault(string uuid, string name,string masterKey, string iv, string salt, long version);
        bool UpdateVaultCloudProvider(int cloudProvider);
        bool UpdateShelterVault(string uuid, string name, string masterKey, string iv, string salt, long version);
        bool IsMasterKeyValid(string masterKey);
        bool InsertCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel);
        bool UpdateCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel);
        bool DeleteCredentials(string uuid);
        bool DeleteVault(string uuid);
        IEnumerable<ShelterVaultCredentialsModel> GetAllCredentials(string shelterVaultUuid);
        IEnumerable<ShelterVaultCredentialsModel> GetAllActiveCredentials(string shelterVaultUuid);
        ShelterVaultCredentialsModel GetCredentialsByUUID(string uuid);
        IEnumerable<ShelterVaultModel> GetAllActiveVaults();
        ShelterVaultModel GetVaultByUUID(string uuid);
        bool UpsertCloudConfiguration(string name, string encryptedValues, string iv);
        bool UpsertSyncStatus(string name, long timestamp, bool isSyncEnabled, CloudSyncStatus cloudSyncStatus);
        bool UpdateSyncStatus(string name, CloudSyncStatus cloudSyncStatus);
        bool UpdateSyncTimestamp(string name, long timestamp);
        ShelterVaultCloudConfigModel GetCloudConfiguration(string name);
        ShelterVaultSyncStatusModel GetSyncStatus(string name);
        void SetDbName(string dbName);
        string GetDefaultShelterVaultDBPath();
        string GetCurrentUUIDVault();
        ShelterVaultModel GetCurrentVault();
    }

    public class ShelterVaultLocalStorage : IShelterVaultLocalStorage
    {
        private string _dbName = "ShelterVault.db";
        private string _userPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private string _shelterVaultPath => Path.Combine(_userPath, ".sheltervault");
        private string _dbPath => Path.Combine(_shelterVaultPath, _dbName);
        private string _dbConnectionString => $"Data Source={_dbPath}";

        public bool DBExists() => GetAllActiveVaults().Count() > 0;

        public void SetDbName(string dbName)
        {
            _dbName = $"{dbName}.db";
        }

        public bool CreateShelterVault(string uuid,string name, string masterKey, string iv, string salt, long version)
        {
            try
            {
                using (var connection = new SqliteConnection(_dbConnectionString))
                {
                    connection.Open();

                    string createShelterVaultMasterKeyTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault (
                            uuid TEXT PRIMARY KEY,
                            name TEXT NOT NULL,
                            masterKeyHash TEXT NOT NULL,
                            iv TEXT NOT NULL,
                            salt TEXT NOT NULL,
                            version INTEGER NOT NULL,
                            cloudProvider INTEGER NOT NULL,
                            UNIQUE(name)
                    )";

                    string createShelterVaultEncryptedCredentialsTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault_credentials (
                            uuid TEXT PRIMARY KEY,
                            encryptedValues TEXT NOT NULL,
                            iv TEXT NOT NULL,
                            shelterVaultUuid TEXT NOT NULL,
                            version INTEGER NOT NULL,
                            FOREIGN KEY(shelterVaultUuid) REFERENCES shelter_vault(uuid)
                    )";

                    string createShelterVaultEncryptedCloudConfigurationTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault_cloud_config (
                            name TEXT PRIMARY KEY,
                            encryptedValues TEXT NOT NULL,
                            iv TEXT NOT NULL
                    )";

                    string createShelterVaultSyncStatusTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault_sync_status (
                            name TEXT PRIMARY KEY,
                            timestamp INTEGER NOT NULL,
                            isSyncEnabled BOOLEAN NOT NULL CHECK (isSyncEnabled IN (0, 1)),
                            syncStatus INTEGER NOT NULL
                    )";

                    string insertMasterKeyQuery = @"
                        INSERT INTO shelter_vault
                        VALUES ($uuid, $name, $masterKeyHash, $iv, $salt, $version, 0)
                    ";

                    using (var command = new SqliteCommand(createShelterVaultMasterKeyTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(createShelterVaultEncryptedCredentialsTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(createShelterVaultEncryptedCloudConfigurationTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(createShelterVaultSyncStatusTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(insertMasterKeyQuery, connection))
                    {
                        command.Parameters.AddWithValue("$uuid", uuid);
                        command.Parameters.AddWithValue("$name", name);
                        command.Parameters.AddWithValue("$masterKeyHash", masterKey);
                        command.Parameters.AddWithValue("$iv", iv);
                        command.Parameters.AddWithValue("$salt", salt);
                        command.Parameters.AddWithValue("$version", version);
                        command.Parameters.AddWithValue("$cloudProvider", 0);
                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateVaultCloudProvider(int cloudProvider)
        {
            try
            {
                using (var connection = new SqliteConnection(_dbConnectionString))
                {
                    connection.Open();

                    string insertMasterKeyQuery = @"
                        UPDATE shelter_vault
                        SET cloudProvider=$cloudProvider
                    ";

                    using (var command = new SqliteCommand(insertMasterKeyQuery, connection))
                    {
                        command.Parameters.AddWithValue("$cloudProvider", cloudProvider);
                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateShelterVault(string uuid, string name, string masterKey, string iv, string salt, long version)
        {
            //TODO: Credentials must be decrypted with the old key and encrypted with the new one, it will be an expensive operation
            try
            {
                using (var connection = new SqliteConnection(_dbConnectionString))
                {
                    connection.Open();

                    string createShelterVaultMasterKeyTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault (
                            uuid TEXT PRIMARY KEY,
                            name TEXT NOT NULL,
                            masterKeyHash TEXT NOT NULL,
                            iv TEXT NOT NULL,
                            salt TEXT NOT NULL,
                            version INTEGER NOT NULL,
                            cloudProvider INTEGER NOT NULL,
                            UNIQUE(name)
                    )";

                    string createShelterVaultEncryptedCredentialsTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault_credentials (
                            uuid TEXT PRIMARY KEY,
                            encryptedValues TEXT NOT NULL,
                            iv TEXT NOT NULL,
                            shelterVaultUuid TEXT NOT NULL,
                            version INTEGER NOT NULL,
                            FOREIGN KEY(shelterVaultUuid) REFERENCES shelter_vault(uuid)
                    )";

                    string createShelterVaultEncryptedCloudConfigurationTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault_cloud_config (
                            name TEXT PRIMARY KEY,
                            encryptedValues TEXT NOT NULL,
                            iv TEXT NOT NULL
                    )";

                    string updateMasterKeyQuery = @"
                        UPDATE shelter_vault SET uuid=$uuid, name=$name, masterKeyHash=$masterKeyHash, iv=$iv, salt=$salt, version=$version
                        WHERE uuid=$uuid
                    ";

                    using (var command = new SqliteCommand(createShelterVaultMasterKeyTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(createShelterVaultEncryptedCredentialsTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(createShelterVaultEncryptedCloudConfigurationTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(updateMasterKeyQuery, connection))
                    {
                        command.Parameters.AddWithValue("$uuid", uuid);
                        command.Parameters.AddWithValue("$name", name);
                        command.Parameters.AddWithValue("$masterKeyHash", masterKey);
                        command.Parameters.AddWithValue("$iv", iv);
                        command.Parameters.AddWithValue("$salt", salt);
                        command.Parameters.AddWithValue("$version", version);
                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsMasterKeyValid(string masterKey)
        {
            if (string.IsNullOrWhiteSpace(masterKey)) return false;

            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT count(*)
                    FROM shelter_vault
                    WHERE masterKeyHash = $masterKeyHash
                ";
                command.Parameters.AddWithValue("$masterKeyHash", masterKey);

                object result = command.ExecuteScalar();
                return result != null && int.Parse(result.ToString()) == 1;
            }
        }

        public bool InsertCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO shelter_vault_credentials
                    VALUES($uuid, $encryptedValues, $iv, $shelterVaultUuid, $version)
                ";
                command.Parameters.AddWithValue("uuid", shelterVaultCredentialsModel.UUID);
                command.Parameters.AddWithValue("$encryptedValues", shelterVaultCredentialsModel.EncryptedValues);
                command.Parameters.AddWithValue("$iv", shelterVaultCredentialsModel.Iv);
                command.Parameters.AddWithValue("$shelterVaultUuid", shelterVaultCredentialsModel.ShelterVaultUuid);
                command.Parameters.AddWithValue("$version", shelterVaultCredentialsModel.Version);

                int result = command.ExecuteNonQuery();

                return result > 0;
            }
        }

        public bool UpdateCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE shelter_vault_credentials
                    SET
                    encryptedValues=$encryptedValues, iv=$iv, shelterVaultUuid=$shelterVaultUuid, version=$version
                    WHERE uuid=$uuid
                ";
                command.Parameters.AddWithValue("$encryptedValues", shelterVaultCredentialsModel.EncryptedValues);
                command.Parameters.AddWithValue("$iv", shelterVaultCredentialsModel.Iv);
                command.Parameters.AddWithValue("$shelterVaultUuid", shelterVaultCredentialsModel.ShelterVaultUuid);
                command.Parameters.AddWithValue("$version", shelterVaultCredentialsModel.Version);
                command.Parameters.AddWithValue("$uuid", shelterVaultCredentialsModel.UUID);

                int result = command.ExecuteNonQuery();
                return result == 1;
            }
        }

        public bool DeleteCredentials(string uuid)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    DELETE FROM shelter_vault_credentials
                    WHERE uuid=$uuid
                ";
                command.Parameters.AddWithValue("uuid", uuid);

                int result = command.ExecuteNonQuery();
                return result == 1;
            }
        }

        public bool DeleteCredentialsByVaultId(string shelterVaultUuid)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    DELETE FROM shelter_vault_credentials
                    WHERE shelterVaultUuid=$shelterVaultUuid
                ";
                command.Parameters.AddWithValue("shelterVaultUuid", shelterVaultUuid);

                int result = command.ExecuteNonQuery();
                return result > 1;
            }
        }

        public bool DeleteVault(string uuid)
        {
            int vaultsDeleted = 0;
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    DELETE FROM shelter_vault
                    WHERE uuid=$uuid
                ";
                command.Parameters.AddWithValue("uuid", uuid);

                vaultsDeleted = command.ExecuteNonQuery();
            }

            return DeleteCredentialsByVaultId(uuid) || vaultsDeleted > 0;
        }

        public IEnumerable<ShelterVaultCredentialsModel> GetAllCredentials(string shelterVaultUuid)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT * FROM shelter_vault_credentials
                    WHERE shelterVaultUuid=$shelterVaultUuid
                ";

                IEnumerable<ShelterVaultCredentialsModel> results = connection.Query<ShelterVaultCredentialsModel>(query, new { shelterVaultUuid });
                return results;
            }
        }

        public IEnumerable<ShelterVaultCredentialsModel> GetAllActiveCredentials(string shelterVaultUuid)
        {
            return GetAllCredentials(shelterVaultUuid).Where(c => c.Version > 0);
        }

        public ShelterVaultCredentialsModel GetCredentialsByUUID(string uuid)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT * FROM shelter_vault_credentials
                    WHERE uuid=$uuid
                ";

                ShelterVaultCredentialsModel result = connection.QueryFirstOrDefault<ShelterVaultCredentialsModel>(query, new { uuid });
                return result;
            }
        }

        public IEnumerable<ShelterVaultModel> GetAllActiveVaults()
        {
            var ext = new List<string> { "db"};
            var vaultPaths = Directory
                .EnumerateFiles(_shelterVaultPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));
            IList<ShelterVaultModel> result = new List<ShelterVaultModel>();

            string query = @"
                SELECT * FROM shelter_vault
                WHERE version > 0
            ";

            foreach (var path in vaultPaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                SetDbName(fileName);
                using (var connection = new SqliteConnection(_dbConnectionString))
                {
                    connection.Open();
                    ShelterVaultModel vault = connection.QueryFirstOrDefault<ShelterVaultModel>(query);
                    if (vault != null)
                        result.Add(vault);
                }
            }

            return result;
        }

        public ShelterVaultModel GetCurrentVault()
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT * FROM shelter_vault
                ";

                ShelterVaultModel result = connection.QueryFirst<ShelterVaultModel>(query);
                return result;
            }
        }

        public ShelterVaultModel GetVaultByUUID(string uuid)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT * FROM shelter_vault
                    WHERE uuid=$uuid
                ";

                ShelterVaultModel result = connection.QueryFirst<ShelterVaultModel>(query, new { uuid });
                return result;
            }
        }

        public bool UpsertCloudConfiguration(string name, string encryptedValues, string iv)
        {
            ShelterVaultCloudConfigModel model = GetCloudConfiguration(name);
            string queryString = string.Empty;
            if (model == null)
            {
                queryString = @"
                    INSERT INTO shelter_vault_cloud_config
                    VALUES($name, $encryptedValues, $iv)
                ";
            }
            else
            {
                queryString = @"
                    UPDATE shelter_vault_cloud_config
                    SET
                    encryptedValues=$encryptedValues, iv=$iv
                    WHERE name=$name
                ";
            }

            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = queryString;
                command.Parameters.AddWithValue("$encryptedValues", encryptedValues);
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$iv", iv);

                int result = command.ExecuteNonQuery();
                return result == 1;
            }
        }

        public bool UpsertSyncStatus(string name, long timestamp, bool isSyncEnabled, CloudSyncStatus cloudSyncStatus)
        {
            ShelterVaultSyncStatusModel model = GetSyncStatus(name);
            string queryString = string.Empty;
            if (model == null)
            {
                queryString = @"
                    INSERT INTO shelter_vault_sync_status
                    VALUES($name, $timestamp, $isSyncEnabled, $cloudSyncStatus)
                ";
            }
            else
            {
                queryString = @"
                    UPDATE shelter_vault_sync_status
                    SET
                    timestamp=$timestamp, isSyncEnabled=$isSyncEnabled, syncStatus=$cloudSyncStatus
                    WHERE name=$name
                ";
            }

            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = queryString;
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$timestamp", timestamp);
                command.Parameters.AddWithValue("$isSyncEnabled", isSyncEnabled ? 1 : 0);
                command.Parameters.AddWithValue("$cloudSyncStatus", (int)cloudSyncStatus);

                int result = command.ExecuteNonQuery();
                return result == 1;
            }
        }

        public bool UpdateSyncTimestamp(string name, long timestamp)
        {
            string queryString = @"
                UPDATE shelter_vault_sync_status
                SET
                timestamp=$timestamp
                WHERE name=$name
            ";

            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = queryString;
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$timestamp", timestamp);

                int result = command.ExecuteNonQuery();
                return result == 1;
            }
        }

        public bool UpdateSyncStatus(string name, CloudSyncStatus cloudSyncStatus)
        {
            string queryString = @"
                UPDATE shelter_vault_sync_status
                SET
                syncStatus=$cloudSyncStatus
                WHERE name=$name
            ";

            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = queryString;
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$cloudSyncStatus", cloudSyncStatus);

                int result = command.ExecuteNonQuery();
                return result == 1;
            }
        }

        public ShelterVaultCloudConfigModel GetCloudConfiguration(string name)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT * FROM shelter_vault_cloud_config
                    WHERE name=$name
                ";

                ShelterVaultCloudConfigModel result = connection.QueryFirstOrDefault<ShelterVaultCloudConfigModel>(query, new { name });
                return result;
            }
        }

        public ShelterVaultSyncStatusModel GetSyncStatus(string name)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT * FROM shelter_vault_sync_status
                    WHERE name=$name
                ";

                ShelterVaultSyncStatusModel result = connection.QueryFirstOrDefault<ShelterVaultSyncStatusModel>(query, new { name });
                return result ?? new();
            }
        }

        public string GetDefaultShelterVaultDBPath()
        {
            return _shelterVaultPath;
        }

        public string GetCurrentUUIDVault()
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT * FROM shelter_vault
                ";

                ShelterVaultModel result = connection.QueryFirst<ShelterVaultModel>(query);
                return result.UUID;
            }
        }
    }
}
