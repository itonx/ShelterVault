using ShelterVault.Models;
using System.Collections.Generic;

namespace ShelterVault.DataLayer
{
    public interface IShelterVaultCredentials
    {
        bool InsertCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel);
        bool UpdateCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel);
        bool DeleteCredentials(string uuid);
        IEnumerable<ShelterVaultCredentialsModel> GetAllCredentials(string shelterVaultUuid);
        IEnumerable<ShelterVaultCredentialsModel> GetAllActiveCredentials(string shelterVaultUuid);
        ShelterVaultCredentialsModel GetCredentialsByUUID(string uuid);
        bool DeleteCredentialsByVaultId(string shelterVaultUuid);
    }

    public class ShelterVaultCredentials : IShelterVaultCredentials
    {
        private readonly IShelterVaultLocalDb _shelterVaultLocalDb;

        public ShelterVaultCredentials(IShelterVaultLocalDb shelterVaultLocalDb)
        {
            _shelterVaultLocalDb = shelterVaultLocalDb;
        }

        public bool InsertCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel)
        {
            string query = @"
                INSERT INTO shelter_vault_credentials
                VALUES($uuid, $encryptedValues, $iv, $shelterVaultUuid, $version)
            ";
            object param = new
            {
                uuid = shelterVaultCredentialsModel.UUID,
                encryptedValues = shelterVaultCredentialsModel.EncryptedValues,
                iv = shelterVaultCredentialsModel.Iv,
                shelterVaultUuid = shelterVaultCredentialsModel.ShelterVaultUuid,
                version = shelterVaultCredentialsModel.Version
            };

            int insertedCredentials = _shelterVaultLocalDb.Execute(query, param);
            return insertedCredentials > 0;
        }

        public bool UpdateCredentials(ShelterVaultCredentialsModel shelterVaultCredentialsModel)
        {
            string query = @"
                UPDATE shelter_vault_credentials
                SET
                encryptedValues=$encryptedValues, iv=$iv, shelterVaultUuid=$shelterVaultUuid, version=$version
                WHERE uuid=$uuid
            ";
            object param = new
            {
                encryptedValues = shelterVaultCredentialsModel.EncryptedValues,
                iv = shelterVaultCredentialsModel.Iv,
                shelterVaultUuid = shelterVaultCredentialsModel.ShelterVaultUuid,
                version = shelterVaultCredentialsModel.Version,
                uuid = shelterVaultCredentialsModel.UUID
            };

            int updatedCredentials = _shelterVaultLocalDb.Execute(query, param);
            return updatedCredentials == 1;
        }

        public bool DeleteCredentials(string uuid)
        {
            string query = @"
                DELETE FROM shelter_vault_credentials
                WHERE uuid=$uuid
            ";
            int credentialsDeleted = _shelterVaultLocalDb.Execute(query, new { uuid });
            return credentialsDeleted == 1;
        }

        public bool DeleteCredentialsByVaultId(string shelterVaultUuid)
        {
            string query = @"
                DELETE FROM shelter_vault_credentials
                WHERE shelterVaultUuid=$shelterVaultUuid
            ";
            int credentialsDeleted = _shelterVaultLocalDb.Execute(query, new { shelterVaultUuid });
            return credentialsDeleted > 0;
        }

        public IEnumerable<ShelterVaultCredentialsModel> GetAllCredentials(string shelterVaultUuid)
        {
            string query = @"
                SELECT * FROM shelter_vault_credentials
                WHERE shelterVaultUuid=$shelterVaultUuid
            ";
            IEnumerable<ShelterVaultCredentialsModel> results = _shelterVaultLocalDb.Query<ShelterVaultCredentialsModel>(query, new { shelterVaultUuid });
            return results;
        }

        public IEnumerable<ShelterVaultCredentialsModel> GetAllActiveCredentials(string shelterVaultUuid)
        {
            string query = @"
                SELECT * FROM shelter_vault_credentials
                WHERE shelterVaultUuid=$shelterVaultUuid
                AND version > 0
            ";
            IEnumerable<ShelterVaultCredentialsModel> results = _shelterVaultLocalDb.Query<ShelterVaultCredentialsModel>(query, new { shelterVaultUuid });
            return results;
        }

        public ShelterVaultCredentialsModel GetCredentialsByUUID(string uuid)
        {
            string query = @"
                SELECT * FROM shelter_vault_credentials
                WHERE uuid=$uuid
            ";

            ShelterVaultCredentialsModel result = _shelterVaultLocalDb.QueryFirstOrDefault<ShelterVaultCredentialsModel>(query, new { uuid });
            return result;
        }
    }
}