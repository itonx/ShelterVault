namespace ShelterVault.Interfaces
{
    public interface IVaultCreatorManager
    {
        bool CreateVault(string uuid, string name, string masterKey);
        bool CreateVault(string uuid, string name, string masterKey, out byte[] vaultKey);
        bool VaultExists(string dbName);
    }
}
