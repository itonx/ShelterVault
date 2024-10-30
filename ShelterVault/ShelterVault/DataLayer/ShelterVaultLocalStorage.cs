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
        bool CreateShelterVault(string masterKey, string salt);
        bool IsMasterKeyValid(string masterKey);
        bool InsertCredential(Credential credential);
        bool UpdateCredential(Credential credential);
        bool DeleteCredential(string uuid);
        IEnumerable<Credential> GetAllCredentials();
        Credential GetCredentialByUUID(string uuid);
        string GetMasterKeySalt();
    }

    public class ShelterVaultLocalStorage : IShelterVaultLocalStorage
    {
        private readonly string _dbName = "ShelterVault.db";
        private string _userPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private string _dbPath => Path.Combine(_userPath, _dbName);
        private string _dbConnectionString => $"Data Source={_dbPath}";
        private readonly string _createShelterVaultMasterKeySaltTableQuery = @"
            CREATE TABLE IF NOT EXISTS shelter_vault_master_key_salt (
            salt TEXT NOT NULL
        )";
        private readonly string _insertMasterKeySaltQuery = @"
            INSERT INTO shelter_vault_master_key_salt
            VALUES ($salt)
        ";
        public bool DBExists() => File.Exists(_dbPath);

        public bool CreateShelterVault(string masterKey, string salt)
        {
            try
            {
                using (var connection = new SqliteConnection(_dbConnectionString))
                {
                    connection.Open();

                    string createShelterVaultMasterKeyTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault_master_key (
                            hash TEXT NOT NULL
                    )";

                    string createShelterVaultEncryptedCredentialsTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault_encrypted_credentials (
                            uuid TEXT PRIMARY KEY,
                            title TEXT NOT NULL,
                            username TEXT,
                            encryptedPassword TEXT NOT NULL,
                            initializationVector TEXT NOT NULL,
                            url TEXT,
                            notes TEXT
                    )";

                    string insertMasterKeyQuery = @"
                        INSERT INTO shelter_vault_master_key
                        VALUES ($hash)
                    ";

                    using (var command = new SqliteCommand(createShelterVaultMasterKeyTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(_createShelterVaultMasterKeySaltTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(createShelterVaultEncryptedCredentialsTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(insertMasterKeyQuery, connection))
                    {
                        command.Parameters.AddWithValue("$hash", masterKey.ToSHA256Base64());
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(_insertMasterKeySaltQuery, connection))
                    {
                        command.Parameters.AddWithValue("$salt", salt.ToBase64Encoded());
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
                    FROM shelter_vault_master_key
                    WHERE hash = $hash
                ";
                command.Parameters.AddWithValue("$hash", masterKey.ToSHA256Base64());

                object result = command.ExecuteScalar();
                return result != null && int.Parse(result.ToString()) == 1;
            }
        }

        public bool InsertCredential(Credential credential)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO shelter_vault_encrypted_credentials
                    VALUES($uuid, $title, $username, $encryptedPassword, $initializationVector, $url, $notes)
                ";
                string uuid = Guid.NewGuid().ToString();
                command.Parameters.AddWithValue("uuid", uuid);
                command.Parameters.AddWithValue("$title", credential.Title);
                command.Parameters.AddWithValue("$username", credential.Username);
                command.Parameters.AddWithValue("$encryptedPassword", credential.EncryptedPassword);
                command.Parameters.AddWithValue("$initializationVector", credential.InitializationVector);
                command.Parameters.AddWithValue("$url", credential.Url);
                command.Parameters.AddWithValue("$notes", credential.Notes);

                int result = command.ExecuteNonQuery();
                if (result > 0) credential.UUID = uuid;

                return result > 0;
            }
        }

        public bool UpdateCredential(Credential credential)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE shelter_vault_encrypted_credentials
                    SET
                    title=$title, username=$username, encryptedPassword=$encryptedPassword, initializationVector=$initializationVector, url=$url, notes=$notes
                    WHERE uuid=$uuid
                ";
                command.Parameters.AddWithValue("$title", credential.Title);
                command.Parameters.AddWithValue("$username", credential.Username);
                command.Parameters.AddWithValue("$encryptedPassword", credential.EncryptedPassword);
                command.Parameters.AddWithValue("$initializationVector", credential.InitializationVector);
                command.Parameters.AddWithValue("$url", credential.Url);
                command.Parameters.AddWithValue("$notes", credential.Notes);
                command.Parameters.AddWithValue("uuid", credential.UUID);

                int result = command.ExecuteNonQuery();
                return result == 1;
            }
        }

        public bool DeleteCredential(string uuid)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    DELETE FROM shelter_vault_encrypted_credentials
                    WHERE uuid=$uuid
                ";
                command.Parameters.AddWithValue("uuid", uuid);

                int result = command.ExecuteNonQuery();
                return result == 1;
            }
        }

        public IEnumerable<Credential> GetAllCredentials()
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT * FROM shelter_vault_encrypted_credentials
                ";

                IEnumerable<Credential> results = connection.Query<Credential>(query);
                return results;
            }
        }

        public Credential GetCredentialByUUID(string uuid)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT * FROM shelter_vault_encrypted_credentials
                    WHERE uuid=$uuid
                ";

                Credential result = connection.QueryFirst<Credential>(query, new { uuid });
                return result;
            }
        }

        public string GetMasterKeySalt()
        {
            CreateMasterKeySaltTableIfNotExists();
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                string query = @"
                    SELECT salt FROM shelter_vault_master_key_salt LIMIT 1
                ";

                string result = connection.QueryFirstOrDefault<string>(query);
                if (result == null)
                {
                    InsertMasterKeySalt();
                    result = connection.QueryFirstOrDefault<string>(query);
                }
                return result.ToBase64Decoded();
            }
        }

        private void CreateMasterKeySaltTableIfNotExists()
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(_createShelterVaultMasterKeySaltTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void InsertMasterKeySalt()
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                using (var command = new SqliteCommand(_insertMasterKeySaltQuery, connection))
                {
                    command.Parameters.AddWithValue("$salt", Guid.NewGuid().ToString().ToBase64Encoded());
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
