using ShelterVault.Models;

namespace ShelterVault.Interfaces
{
    public interface IShelterVaultStateService
    {
        (byte[], byte[]) GetLocalEncryptionValues();
        void SetVault(ShelterVaultModel shelterVaultModel);
        void SetVault(ShelterVaultModel shelterVaultModel, string masterKey);
        void SetVault(ShelterVaultModel shelterVaultModel, byte[] encryptionKey);
        void ResetState();
        ShelterVaultModel ShelterVault { get; }
        void SetDialogStatus(bool isDialogOpen);
        bool GetDialogStatus();
    }
}
