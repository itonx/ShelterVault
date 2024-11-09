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
    internal interface IMasterKeyValidatorManager
    {
        bool IsValid(string masterKey, ShelterVaultModel shelterVaultModel);
    }

    internal class MasterKeyValidatorManager : IMasterKeyValidatorManager
    {
        private readonly IEncryptionService _encryptionService;

        public MasterKeyValidatorManager(IShelterVaultLocalStorage shelterVaultLocalStorage, IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public bool IsValid(string masterKey, ShelterVaultModel shelterVaultModel)
        {
            byte[] masterKeyBytes = masterKey.GetBytes();
            string expectedMasterKeyHash = _encryptionService.DecryptAes(shelterVaultModel, masterKeyBytes);

            return expectedMasterKeyHash != null && expectedMasterKeyHash.Equals(masterKey.ToSHA256Hex());
        }
    }
}
