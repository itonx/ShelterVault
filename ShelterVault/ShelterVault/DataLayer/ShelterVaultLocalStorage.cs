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
        bool CreateShelterVault(string name,string masterKey, string iv, string salt);
        bool IsMasterKeyValid(string masterKey);
        bool InsertCredential(Credential credential);
        bool UpdateCredential(Credential credential);
        bool DeleteCredential(string uuid);
        IEnumerable<Credential> GetAllCredentials();
        Credential GetCredentialByUUID(string uuid);
        IEnumerable<ShelterVaultModel> GetVaults();
    }

    public class ShelterVaultLocalStorage : IShelterVaultLocalStorage
    {
        private readonly string _dbName = "ShelterVault.db";
        private string _userPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private string _dbPath => Path.Combine(_userPath, _dbName);
        private string _dbConnectionString => $"Data Source={_dbPath}";

        public bool DBExists() => File.Exists(_dbPath);

        public bool CreateShelterVault(string name, string masterKey, string iv, string salt)
        {
            try
            {
                using (var connection = new SqliteConnection(_dbConnectionString))
                {
                    connection.Open();

                    string createShelterVaultMasterKeyTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault (
                            name TEXT NOT NULL,
                            masterKeyHash TEXT NOT NULL,
                            iv TEXT NOT NULL,
                            salt TEXT NOT NULL,
                            UNIQUE(name)
                    )";

                    string createShelterVaultEncryptedCredentialsTableQuery = @"
                        CREATE TABLE IF NOT EXISTS shelter_vault_credentials (
                            uuid TEXT PRIMARY KEY,
                            title TEXT NOT NULL,
                            username TEXT,
                            encryptedPassword TEXT NOT NULL,
                            iv TEXT NOT NULL,
                            url TEXT,
                            notes TEXT
                    )";

                    string insertMasterKeyQuery = @"
                        INSERT INTO shelter_vault
                        VALUES ($name, $masterKeyHash, $iv, $salt)
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
                        command.Parameters.AddWithValue("$name", name);
                        command.Parameters.AddWithValue("$masterKeyHash", masterKey);
                        command.Parameters.AddWithValue("$iv", iv);
                        command.Parameters.AddWithValue("$salt", salt);
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

        public bool InsertCredential(Credential credential)
        {
            using (var connection = new SqliteConnection(_dbConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO shelter_vault_credentials
                    VALUES($uuid, $title, $username, $encryptedPassword, $iv, $url, $notes)
                ";
                string uuid = Guid.NewGuid().ToString();
                command.Parameters.AddWithValue("uuid", uuid);
                command.Parameters.AddWithValue("$title", credential.Title);
                command.Parameters.AddWithValue("$username", credential.Username);
                command.Parameters.AddWithValue("$encryptedPassword", credential.EncryptedPassword);
                command.Parameters.AddWithValue("$iv", credential.Iv);
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
                    UPDATE shelter_vault_credentials
                    SET
                    title=$title, username=$username, encryptedPassword=$encryptedPassword, iv=$iv, url=$url, notes=$notes
                    WHERE uuid=$uuid
                ";
                command.Parameters.AddWithValue("$title", credential.Title);
                command.Parameters.AddWithValue("$username", credential.Username);
                command.Parameters.AddWithValue("$encryptedPassword", credential.EncryptedPassword);
                command.Parameters.AddWithValue("$iv", credential.Iv);
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
                    DELETE FROM shelter_vault_credentials
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
                    SELECT * FROM shelter_vault_credentials
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
                    SELECT * FROM shelter_vault_credentials
                    WHERE uuid=$uuid
                ";

                Credential result = connection.QueryFirst<Credential>(query, new { uuid });
                return result;
            }
        }

        public IEnumerable<ShelterVaultModel> GetVaults()
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
    }
}
