using ShelterVault.Models;
using ShelterVault.Shared.Extensions;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ShelterVault.Services
{
    public interface IShelterVaultStateService
    {
        byte[] GetMasterKeyUnprotected();
        byte[] GetMasterKeySaltUnprotected();
        void SetVault(ShelterVaultModel shelterVaultModel);
        void SetVault(ShelterVaultModel shelterVaultModel, string masterKey);
        void ResetState();
        ShelterVaultModel ShelterVault { get; }
    }

    public class ShelterVaultStateService : IShelterVaultStateService
    {
        private byte[] _inMemoryMasterKeyProtected;
        private byte[] _inMemoryMasterKeySaltProtected;

        public ShelterVaultModel ShelterVault { get; private set; }

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
            SetVault(shelterVaultModel);
            ProtectMasterKey(masterKey.GetBytes(), shelterVaultModel.Salt.FromBase64ToBytes());
        }

        public void SetVault(ShelterVaultModel shelterVaultModel)
        {
            ShelterVault = shelterVaultModel;
        }

        public void ResetState()
        {
            _inMemoryMasterKeyProtected = Array.Empty<byte>();
            _inMemoryMasterKeySaltProtected = Array.Empty<byte>();
            ShelterVault = new();
        }

        private void ProtectMasterKey(byte[] masterKey, byte[] masterKeySalt)
        {
            _inMemoryMasterKeyProtected = ProtectedData.Protect(masterKey, null, DataProtectionScope.CurrentUser);
            _inMemoryMasterKeySaltProtected = ProtectedData.Protect(masterKeySalt, null, DataProtectionScope.CurrentUser);
        }
    }
}
