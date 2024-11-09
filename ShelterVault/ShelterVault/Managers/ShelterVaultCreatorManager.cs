using ShelterVault.DataLayer;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Managers
{
    internal interface IShelterVaultCreatorManager
    {
        bool CreateVault(string uuid, string name, string masterKey, string salt);
    }

    internal class ShelterVaultCreatorManager : IShelterVaultCreatorManager
    {

        private readonly IEncryptionService _encryptionService;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;

        public ShelterVaultCreatorManager(IEncryptionService encryptionService, IShelterVaultLocalStorage shelterVaultLocalStorage)
        {
            _encryptionService = encryptionService;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
        }

        public bool CreateVault(string uuid, string name, string masterKey, string salt)
        {
            byte[] masterKeyBytes = masterKey.GetBytes();
            string masterKeyHash = masterKey.ToSHA256Hex();
            byte[] saltBytes = salt.GetBytes();

            (byte[] encryptedMasterKeyHash, byte[] iv) = _encryptionService.EncryptAes(masterKeyHash, masterKeyBytes, saltBytes);

            return _shelterVaultLocalStorage.CreateShelterVault(uuid, name, encryptedMasterKeyHash.ToBase64(), iv.ToBase64(), saltBytes.ToBase64());
        }
    }
}
