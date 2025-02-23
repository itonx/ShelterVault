using Dapper;
using Microsoft.Data.Sqlite;
using ShelterVault.Models;
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
        bool UpdateShelterVault(string uuid, string name, string masterKey, string iv, string salt, long version);
        bool IsMasterKeyValid(string masterKey);
        bool InsertCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel);
        bool UpdateCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel);
        bool DeleteCredentials(string uuid);
        bool DeleteVault(string uuid);
        IEnumerable<ShelterVaultCredentialsModel> GetAllCredentials(string shelterVaultUuid);
        IEnumerable<ShelterVaultCredentialsModel> GetAllActiveCredentials(string shelterVaultUuid);
        ShelterVaultCredentialsModel GetCredentialsByUUID(string uuid);
        IEnumerable<ShelterVaultModel> GetAllVaults();
        IEnumerable<ShelterVaultModel> GetAllActiveVaults();
        ShelterVaultModel GetVaultByUUID(string uuid);
    }

    public class ShelterVaultLocalStorage : IShelterVaultLocalStorage
    {
        private readonly string _dbName = "ShelterVault.db";
        private string _userPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private string _dbPath => Path.Combine(_userPath, _dbName);
        private string _dbConnectionString => $"Data Source={_dbPath}";

        public bool DBExists() => File.Exists(_dbPath);

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

                    string insertMasterKeyQuery = @"
                        INSERT INTO shelter_vault
                        VALUES ($uuid, $name, $masterKeyHash, $iv, $salt, $version)
                    ";

                    using (var command = new SqliteCommand(createShelterVaultMasterKeyTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(createShelterVaultEncryptedCredentialsTableQuery, connection))
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

        public IEnumerable<ShelterVaultModel> GetAllVaults()
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT * FROM shelter_vault
                ";

                IEnumerable<ShelterVaultModel> result = connection.Query<ShelterVaultModel>(query) ?? Enumerable.Empty<ShelterVaultModel>();
                return result;
            }
        }

        public IEnumerable<ShelterVaultModel> GetAllActiveVaults()
        {
            return GetAllVaults().Where(v => v.Version > 0);
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
    }
}
