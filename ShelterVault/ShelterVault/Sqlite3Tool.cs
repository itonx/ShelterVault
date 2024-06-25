using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault
{
    internal static class Sqlite3Tool
    {
        private static string DB_NAME = "ShelterVault.db";
        private static string _userPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static string _dbPath => Path.Combine(_userPath, DB_NAME);
        private static string _dbConnectionString => $"Data Source={_dbPath}";
        internal static bool DBExists() => File.Exists(_dbPath);

        internal static bool CreateShelterVault(string masterKey)
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
                            title TEXT,
                            username TEXT,
                            encrypted_password TEXT NOT NULL,
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
                        command.Parameters.AddWithValue("$hash", GetSHA256InBase64(ref masterKey));
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

        internal static bool IsMasterKey(string masterKey)
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
                command.Parameters.AddWithValue("$hash", GetSHA256InBase64(ref masterKey));

                object result = command.ExecuteScalar();
                return result != null && int.Parse(result.ToString()) == 1;
            }
        }

        internal static string GetSHA256InBase64(ref string val)
        {
            using(SHA256 sha256 = SHA256.Create())
            {
                byte[] hashValueBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(val));
                return Convert.ToBase64String(hashValueBytes);
            }
        }
    }
}
