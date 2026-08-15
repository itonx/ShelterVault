using ShelterVault.Models;

namespace ShelterVault.Interfaces
{
    public interface IShelterVaultEncryption
    {
        (byte[], byte[]) EncryptAes(string plainText, byte[] key);
        (byte[], byte[]) EncryptAes(string plainText, byte[] key, int? version);
        string DecryptAes(byte[] cipherText, byte[] key, byte[] iv);
        byte[] DecryptAesBytes(byte[] cipherText, byte[] key, byte[] iv);
        string DecryptAes(ShelterVaultCredentialsModel shelterVaultCredentialsModel, byte[] key);
        string DecryptAes(ShelterVaultModel shelterVaultModel, byte[] key);
        string DecryptAes(CredentialsViewItem credentialsViewItem, byte[] key);
        string DecryptAes(ShelterVaultCloudConfigModel shelterVaultCloudConfigModel, byte[] key);
        byte[] DeriveKeyFromPassword(string password, byte[] salt);
    }
}
