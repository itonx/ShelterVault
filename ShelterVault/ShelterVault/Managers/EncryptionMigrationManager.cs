using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;

namespace ShelterVault.Managers
{
    public interface IEncryptionMigrationManager
    {
        Task MigrateEncryptedDataToArgon2Async(long previousVersion, long newVersion);
    }

    public class EncryptionMigrationManager : IEncryptionMigrationManager
    {
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultCredentials _shelterVaultCredentials;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly IEncryptionService _encryptionService;

        public EncryptionMigrationManager(
            IShelterVault shelterVaultLocalDb,
            IShelterVaultCredentials shelterVaultCredentials,
            IShelterVaultStateService shelterVaultStateService,
            IEncryptionService encryptionService
        )
        {
            _shelterVault = shelterVaultLocalDb;
            _shelterVaultCredentials = shelterVaultCredentials;
            _shelterVaultStateService = shelterVaultStateService;
            _encryptionService = encryptionService;
        }

        public async Task MigrateEncryptedDataToArgon2Async(long previousVersion, long newVersion)
        {
            if (previousVersion >= newVersion || previousVersion <= 0 || newVersion <= 0)
            {
                throw new EncryptionMigrationException();
            }

            var vaults = _shelterVault.GetVaults();
            var oldVault = vaults.Where(v => v.Version == previousVersion).FirstOrDefault();
            var newVault = vaults.Where(v => v.Version == newVersion).FirstOrDefault();

            if (oldVault == null || newVault != null)
            {
                throw new EncryptionMigrationException();
            }

            var oldCredentials = _shelterVaultCredentials.GetAllCredentials(oldVault.UUID);
            var decryptedCredentials = new List<Credentials>();
            (byte[] derivedKey, byte[] salt) = _shelterVaultStateService.GetLocalEncryptionValues();

            foreach (var oldCredential in oldCredentials)
            {
                var decryptedValues = _encryptionService.DecryptAes(
                    oldCredential,
                    derivedKey,
                    salt
                );
                decryptedCredentials.Add(new(decryptedValues, oldCredential));
            }
        }
    }
}
