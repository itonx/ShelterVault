using System.Collections.Generic;
using ShelterVault.Models;

namespace ShelterVault.Interfaces
{
    public interface IVaultManager
    {
        IList<VaultModel> GetCurrentVaultWithCredentials();
        ShelterVaultModel GetCurrentVault();
        bool IsValid(
            string masterKey,
            ShelterVaultModel shelterVaultModel,
            out byte[] encryptionKey
        );
    }
}
