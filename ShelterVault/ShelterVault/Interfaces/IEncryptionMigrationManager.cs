using System.Threading.Tasks;

namespace ShelterVault.Interfaces
{
    public interface IEncryptionMigrationManager
    {
        Task MigrateEncryptedDataToArgon2Async(
            long previousVersion,
            long newVersion,
            string masterKey
        );
        bool IsArgonMigrationAvailable();
    }
}
