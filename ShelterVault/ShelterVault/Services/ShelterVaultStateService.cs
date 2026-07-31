using System;
using System.Security.Cryptography;
using ShelterVault.Interfaces;
using ShelterVault.Models;
using ShelterVault.Shared.Extensions;

namespace ShelterVault.Services
{
    public class ShelterVaultStateService : IShelterVaultStateService
    {
        private readonly IShelterVaultEncryption _encryptionService;
        private byte[] _inMemoryDerivedKeyProtected;
        private byte[] _inMemorySaltProtected;
        private bool _isDialogOpen;

        public ShelterVaultStateService(IShelterVaultEncryption encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public ShelterVaultModel ShelterVault { get; private set; }

        private byte[] GetEncryptionKeyUnprotected()
        {
            return ProtectedData.Unprotect(
                _inMemoryDerivedKeyProtected,
                null,
                DataProtectionScope.CurrentUser
            );
        }

        private byte[] GetSaltUnprotected()
        {
            return ProtectedData.Unprotect(
                _inMemorySaltProtected,
                null,
                DataProtectionScope.CurrentUser
            );
        }

        public (byte[], byte[]) GetLocalEncryptionValues()
        {
            return (GetEncryptionKeyUnprotected(), GetSaltUnprotected());
        }

        public void SetVault(ShelterVaultModel shelterVaultModel, string masterKey)
        {
            SetVault(shelterVaultModel);
            byte[] salt = shelterVaultModel.Salt.FromBase64ToBytes();
            byte[] encryptionKeyBytes = _encryptionService.DeriveKeyFromPassword(masterKey, salt);
            ProtectEncryptionValues(encryptionKeyBytes, salt);
        }

        public void SetVault(ShelterVaultModel shelterVaultModel, byte[] encryptionKey)
        {
            SetVault(shelterVaultModel);
            byte[] salt = shelterVaultModel.Salt.FromBase64ToBytes();
            ProtectEncryptionValues(encryptionKey, salt);
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

        private void ProtectEncryptionValues(byte[] encryptionKey, byte[] salt)
        {
            _inMemoryDerivedKeyProtected = ProtectedData.Protect(
                encryptionKey,
                null,
                DataProtectionScope.CurrentUser
            );
            _inMemorySaltProtected = ProtectedData.Protect(
                salt,
                null,
                DataProtectionScope.CurrentUser
            );
        }

        public void SetDialogStatus(bool isDialogOpen)
        {
            _isDialogOpen = isDialogOpen;
        }

        public bool GetDialogStatus()
        {
            return _isDialogOpen;
        }
    }
}
