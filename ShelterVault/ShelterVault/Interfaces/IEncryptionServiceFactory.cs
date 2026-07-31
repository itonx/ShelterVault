namespace ShelterVault.Interfaces
{
    public interface IEncryptionServiceFactory
    {
        IEncryptionService Create(int version);
    }
}
