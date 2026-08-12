using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShelterVault.DataLayer;
using ShelterVault.Factories;
using ShelterVault.Interfaces;
using ShelterVault.Models;

namespace ShelterVault.Managers
{
    public class EncryptionMigrationManager : IEncryptionMigrationManager
    {
        private readonly IShelterVault _shelterVault;
        private readonly IShelterVaultCredentials _shelterVaultCredentials;
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly IEncryptionServiceFactory _encryptionServiceFactory;
        private readonly IVaultCreatorManager _vaultCreatorManager;
        private readonly ICredentialsManager _credentialsManager;

        public EncryptionMigrationManager(
            IShelterVault shelterVaultLocalDb,
            IShelterVaultCredentials shelterVaultCredentials,
            IShelterVaultStateService shelterVaultStateService,
            IEncryptionServiceFactory encryptionServiceFactory,
            IVaultCreatorManager vaultCreatorManager,
            ICredentialsManager credentialsManager
        )
        {
            _shelterVault = shelterVaultLocalDb;
            _shelterVaultCredentials = shelterVaultCredentials;
            _shelterVaultStateService = shelterVaultStateService;
            _encryptionServiceFactory = encryptionServiceFactory;
            _vaultCreatorManager = vaultCreatorManager;
            _credentialsManager = credentialsManager;
        }

        public async Task MigrateEncryptedDataToArgon2Async(
            long previousVersion,
            long newVersion,
            string masterKey
        )
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

            var oldEncryptionService = _encryptionServiceFactory.Create((int)oldVault.Version);

            var oldCredentials = _shelterVaultCredentials.GetAllCredentials(oldVault.UUID);
            var decryptedCredentials = new List<Credentials>();
            (byte[] encryptionKey, byte[] salt) =
                _shelterVaultStateService.GetLocalEncryptionValues();

            foreach (var oldCredential in oldCredentials)
            {
                var decryptedValues = oldEncryptionService.DecryptAes(oldCredential, encryptionKey);
                decryptedCredentials.Add(new(decryptedValues, oldCredential));
            }

            var newEncryptionService = _encryptionServiceFactory.Create((int)newVersion);
            string newVaultUUID = Guid.NewGuid().ToString();
            byte[] newEncryptionKey = null;
            string newVaultName = string.Concat($"(v{newVersion})", oldVault.Name);

            _vaultCreatorManager.CreateVault(
                newVaultUUID,
                newVaultName,
                masterKey,
                out newEncryptionKey
            );

            List<Task> insertCredentialsTasks = new List<Task>();
            foreach (var decryptedCredential in decryptedCredentials)
            {
                decryptedCredential.ShelterVaultUuid = newVaultUUID;
                Task insertCredentialsTask = _credentialsManager.InsertCredentials(
                    decryptedCredential,
                    newEncryptionKey,
                    newVaultName,
                    (int)newVersion
                );
                insertCredentialsTasks.Add(insertCredentialsTask);
            }

            await Task.WhenAll(insertCredentialsTasks);
        }

        public bool IsArgonMigrationAvailable()
        {
            var vault = _shelterVault.GetCurrentVault();
            return vault?.Version == 1;
        }
    }
}
