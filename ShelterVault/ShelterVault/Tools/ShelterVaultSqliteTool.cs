using Dapper;
using Microsoft.Data.Sqlite;
using ShelterVault.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace ShelterVault.Tools
{
    public static class ShelterVaultSqliteTool
    {
        private static readonly string _dbName = "ShelterVault.db";
        private static string _userPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static string _dbPath => Path.Combine(_userPath, _dbName);
        private static string _dbConnectionString => $"Data Source={_dbPath}";

        public static bool DBExists() => File.Exists(_dbPath);

        public static bool CreateShelterVault(string masterKey)
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

                    using (var command = new SqliteCommand(createShelterVaultEncryptedCredentialsTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand(insertMasterKeyQuery, connection))
                    {
                        command.Parameters.AddWithValue("$hash", masterKey.ToSHA256Base64());
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

        public static bool IsMasterKeyValid(string masterKey)
        {
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

        public static bool InsertCredential(Credential credential)
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

        public static bool UpdateCredential(Credential credential)
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

        public static bool DeleteCredential(string uuid)
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

        public static IEnumerable<Credential> GetAllCredentials()
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

        public static Credential GetCredentialByUUID(string uuid)
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
    }
}
 