namespace ShelterVault.Interfaces
{
    public interface IVaultCreatorManager
    {
        bool CreateVault(string uuid, string name, string masterKey);
    }
}
