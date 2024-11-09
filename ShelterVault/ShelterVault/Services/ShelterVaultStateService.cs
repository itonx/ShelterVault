using ShelterVault.Models;
using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.Services
{
    internal interface IShelterVaultStateService
    {
        byte[] GetMasterKeyUnprotected();
        byte[] GetMasterKeySaltUnprotected();
        void SetVault(ShelterVaultModel shelterVaultModel, string masterKey);
        string VaultName { get; }
    }

    internal class ShelterVaultStateService : IShelterVaultStateService
    {
        private byte[] _inMemoryMasterKeyProtected;
        private byte[] _inMemoryMasterKeySaltProtected;

        public string VaultName { get; private set; }

        public byte[] GetMasterKeyUnprotected()
        {
            return ProtectedData.Unprotect(_inMemoryMasterKeyProtected, null, DataProtectionScope.CurrentUser);
        }

        public byte[] GetMasterKeySaltUnprotected()
        {
            return ProtectedData.Unprotect(_inMemoryMasterKeySaltProtected, null, DataProtectionScope.CurrentUser);
        }

        public void SetVault(ShelterVaultModel shelterVaultModel, string masterKey)
        {
            VaultName = shelterVaultModel.Name;
            ProtectMasterKey(masterKey.GetBytes(), shelterVaultModel.Salt.FromBase64ToBytes());
        }

        private void ProtectMasterKey(byte[] masterKey, byte[] masterKeySalt)
        {
            _inMemoryMasterKeyProtected = ProtectedData.Protect(masterKey, null, DataProtectionScope.CurrentUser);
            _inMemoryMasterKeySaltProtected = ProtectedData.Protect(masterKeySalt, null, DataProtectionScope.CurrentUser);
        }
    }
}
