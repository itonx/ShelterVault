using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Managers
{
    interface IMasterKeyValidatorManager
    {
        bool IsValid(string masterKey, ShelterVaultModel shelterVaultModel);
    }

    class MasterKeyValidatorManager : IMasterKeyValidatorManager
    {
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;
        private readonly IEncryptionService _encryptionService;
        private int _attempts = 0;

        public MasterKeyValidatorManager(IShelterVaultLocalStorage shelterVaultLocalStorage, IEncryptionService encryptionService)
        {
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
            _encryptionService = encryptionService;
        }

        public bool IsValid(string masterKey, ShelterVaultModel shelterVaultModel)
        {
            byte[] masterKeyBytes = masterKey.GetBytes();
            string expectedMasterKeyHash = _encryptionService.DecryptAes(shelterVaultModel.MasterKeyHash.FromBase64ToBytes(), masterKeyBytes, shelterVaultModel.Iv.FromBase64ToBytes(), shelterVaultModel.Salt.FromBase64ToBytes());

            if(expectedMasterKeyHash == null) _attempts++;

            return expectedMasterKeyHash != null && expectedMasterKeyHash.Equals(masterKey.ToSHA256Hex());
        }
    }
}
