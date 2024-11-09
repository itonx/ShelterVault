using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.Services
{
    internal interface IMasterKeyService
    {
        byte[] GetMasterKeyUnprotected();
        byte[] GetMasterKeySaltUnprotected();
        void ProtectMasterKey(byte[] masterKey, byte[] masterKeySalt);
    }

    internal class MasterKeyService : IMasterKeyService
    {
        private byte[] _inMemoryMasterKeyProtected;
        private byte[] _inMemoryMasterKeySaltProtected;

        public byte[] GetMasterKeyUnprotected()
        {
            return ProtectedData.Unprotect(_inMemoryMasterKeyProtected, null, DataProtectionScope.CurrentUser);
        }

        public byte[] GetMasterKeySaltUnprotected()
        {
            return ProtectedData.Unprotect(_inMemoryMasterKeySaltProtected, null, DataProtectionScope.CurrentUser);
        }

        public void ProtectMasterKey(byte[] masterKey, byte[] masterKeySalt)
        {
            _inMemoryMasterKeyProtected = ProtectedData.Protect(masterKey, null, DataProtectionScope.CurrentUser);
            _inMemoryMasterKeySaltProtected = ProtectedData.Protect(masterKeySalt, null, DataProtectionScope.CurrentUser);
        }
    }
}
