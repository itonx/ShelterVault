using System.Collections.Generic;

namespace ShelterVault.DataLayer
{
    public static class ShelterVaultQueries
    {
        public static readonly KeyValuePair<string, object>[] CREATE_SHELTER_VAULT_DB =
        [
            new KeyValuePair<string, object>(@"
                CREATE TABLE IF NOT EXISTS shelter_vault (
                    uuid TEXT PRIMARY KEY,
                    name TEXT NOT NULL,
                    encryptedTestValue TEXT NOT NULL,
                    iv TEXT NOT NULL,
                    salt TEXT NOT NULL,
                    version INTEGER NOT NULL,
                    cloudProvider INTEGER NOT NULL,
                    UNIQUE(name)
            )", null),
            new KeyValuePair<string, object>(@"
                CREATE TABLE IF NOT EXISTS shelter_vault_credentials (
                    uuid TEXT PRIMARY KEY,
                    encryptedValues TEXT NOT NULL,
                    iv TEXT NOT NULL,
                    shelterVaultUuid TEXT NOT NULL,
                    version INTEGER NOT NULL,
                    FOREIGN KEY(shelterVaultUuid) REFERENCES shelter_vault(uuid)
            )", null),
            new KeyValuePair<string, object>(@"
                CREATE TABLE IF NOT EXISTS shelter_vault_cloud_config (
                    name TEXT PRIMARY KEY,
                    encryptedValues TEXT NOT NULL,
                    iv TEXT NOT NULL
            )", null),
            new KeyValuePair<string, object>(@"
                CREATE TABLE IF NOT EXISTS shelter_vault_sync_status (
                    name TEXT PRIMARY KEY,
                    timestamp INTEGER NOT NULL,
                    isSyncEnabled BOOLEAN NOT NULL CHECK (isSyncEnabled IN (0, 1)),
                    syncStatus INTEGER NOT NULL
            )", null)
        ];
    }
}
