using ShelterVault.Models;
using ShelterVault.Shared.Enums;

namespace ShelterVault.DataLayer
{
    public interface IShelterVaultSyncStatus
    {
        bool UpsertSyncStatus(string name, long timestamp, bool isSyncEnabled, int cloudSyncStatus);
        bool UpsertSyncStatus(CloudProviderType cloudProviderType, long timestamp, bool isSyncEnabled, CloudSyncStatus cloudSyncStatus);
        bool UpdateSyncStatus(string name, CloudSyncStatus cloudSyncStatus);
        bool UpdateSyncStatus(string name, long timestamp);
        bool UpdateSyncStatus(CloudProviderType cloudProviderType, CloudSyncStatus cloudSyncStatus);
        ShelterVaultSyncStatusModel GetSyncStatus(string name);
        ShelterVaultSyncStatusModel GetSyncStatus(CloudProviderType cloudProviderType);
        bool DisableSync(CloudProviderType cloudProviderType);
        bool UpdateSyncTimestamp(CloudProviderType cloudProviderType, long timestamp);
        ShelterVaultSyncStatusModel GetCurrentCloudSyncInformation(CloudProviderType cloudProviderType);
    }

    public class ShelterVaultSyncStatus : IShelterVaultSyncStatus
    {
        private readonly IShelterVaultLocalDb _shelterVaultLocalDb;

        public ShelterVaultSyncStatus(IShelterVaultLocalDb shelterVaultLocalDb)
        {
            _shelterVaultLocalDb = shelterVaultLocalDb;
        }

        public bool UpsertSyncStatus(string name, long timestamp, bool isSyncEnabled, int cloudSyncStatus)
        {
            ShelterVaultSyncStatusModel model = GetSyncStatus(name);
            string query = string.Empty;
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                query = @"
                    INSERT INTO shelter_vault_sync_status
                    VALUES($name, $timestamp, $isSyncEnabled, $cloudSyncStatus)
                ";
            }
            else
            {
                query = @"
                    UPDATE shelter_vault_sync_status
                    SET
                    timestamp=$timestamp, isSyncEnabled=$isSyncEnabled, syncStatus=$cloudSyncStatus
                    WHERE name=$name
                ";
            }

            int result = _shelterVaultLocalDb.Execute(query, new { name, timestamp, isSyncEnabled = isSyncEnabled ? 1 : 0, cloudSyncStatus = cloudSyncStatus });
            return result == 1;
        }

        public bool UpsertSyncStatus(CloudProviderType cloudProviderType, long timestamp, bool isSyncEnabled, CloudSyncStatus cloudSyncStatus)
        {
            return UpsertSyncStatus(cloudProviderType.ToString(), timestamp, isSyncEnabled, (int)cloudSyncStatus);
        }

        public bool UpdateSyncStatus(string name, long timestamp)
        {
            string query = @"
                UPDATE shelter_vault_sync_status
                SET
                timestamp=$timestamp
                WHERE name=$name
            ";

            int updatedRecords = _shelterVaultLocalDb.Execute(query, new { name, timestamp });
            return updatedRecords == 1;
        }


        public bool UpdateSyncStatus(string name, CloudSyncStatus cloudSyncStatus)
        {
            string query = @"
                UPDATE shelter_vault_sync_status
                SET
                syncStatus=$cloudSyncStatus
                WHERE name=$name
            ";

            int updatedRecords = _shelterVaultLocalDb.Execute(query, new { name, cloudSyncStatus });
            return updatedRecords == 1;
        }

        public bool UpdateSyncStatus(CloudProviderType cloudProviderType, CloudSyncStatus cloudSyncStatus)
        {
            return UpdateSyncStatus(cloudProviderType.ToString(), cloudSyncStatus);
        }

        public bool UpdateSyncTimestamp(CloudProviderType cloudProviderType, long timestamp)
        {
            return UpdateSyncStatus(cloudProviderType.ToString(), timestamp);
        }

        public ShelterVaultSyncStatusModel GetSyncStatus(string name)
        {
            string query = @"
                SELECT * FROM shelter_vault_sync_status
                WHERE name=$name
            ";

            ShelterVaultSyncStatusModel result = _shelterVaultLocalDb.QueryFirstOrDefault<ShelterVaultSyncStatusModel>(query, new { name });
            return result ?? new();
        }

        public ShelterVaultSyncStatusModel GetSyncStatus(CloudProviderType cloudProviderType)
        {
            return GetSyncStatus(cloudProviderType.ToString());
        }

        public bool DisableSync(CloudProviderType cloudProviderType)
        {
            return UpsertSyncStatus(cloudProviderType, 0, false, CloudSyncStatus.None);
        }

        public ShelterVaultSyncStatusModel GetCurrentCloudSyncInformation(CloudProviderType cloudProviderType)
        {
            return GetSyncStatus(cloudProviderType.ToString());
        }
    }
}