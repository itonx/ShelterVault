using ShelterVault.Models;
using ShelterVault.Shared.Extensions;
using System;
using System.Security.Cryptography;

namespace ShelterVault.Services
{
    public interface IShelterVaultStateService
    {
        (byte[], byte[]) GetLocalEncryptionValues();
        void SetVault(ShelterVaultModel shelterVaultModel);
        void SetVault(ShelterVaultModel shelterVaultModel, string masterKey);
        void ResetState();
        ShelterVaultModel ShelterVault { get; }
    }

    public class ShelterVaultStateService : IShelterVaultStateService
    {
        private readonly IEncryptionService _encryptionService;

        public ShelterVaultStateService(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        private byte[] _inMemoryDerivedKeyProtected;
        private byte[] _inMemorySaltProtected;

        public ShelterVaultModel ShelterVault { get; private set; }

        private byte[] GetDerivedKeyUnprotected()
        {
            return ProtectedData.Unprotect(_inMemoryDerivedKeyProtected, null, DataProtectionScope.CurrentUser);
        }

        private byte[] GetSaltUnprotected()
        {
            return ProtectedData.Unprotect(_inMemorySaltProtected, null, DataProtectionScope.CurrentUser);
        }

        public (byte[], byte[]) GetLocalEncryptionValues()
        {
            return (GetDerivedKeyUnprotected(), GetSaltUnprotected());
        }

        public void SetVault(ShelterVaultModel shelterVaultModel, string masterKey)
        {
            SetVault(shelterVaultModel);
            byte[] salt = shelterVaultModel.Salt.FromBase64ToBytes();
            byte[] derivedKey = _encryptionService.DeriveKeyFromPassword(masterKey, salt);
            ProtectEncryptionValues(derivedKey, salt);
        }

        public void SetVault(ShelterVaultModel shelterVaultModel)
        {
            ShelterVault = shelterVaultModel;
        }

        public void ResetState()
        {
            _inMemoryDerivedKeyProtected = Array.Empty<byte>();
            _inMemorySaltProtected = Array.Empty<byte>();
            ShelterVault = new();
        }

        private void ProtectEncryptionValues(byte[] derivedKey, byte[] salt)
        {
            _inMemoryDerivedKeyProtected = ProtectedData.Protect(derivedKey, null, DataProtectionScope.CurrentUser);
            _inMemorySaltProtected = ProtectedData.Protect(salt, null, DataProtectionScope.CurrentUser);
        }
    }
}
