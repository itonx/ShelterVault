using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ShelterVault.DataLayer;
using ShelterVault.Models;
using ShelterVault.Services;

namespace ShelterVault.Managers
{
    public interface IKeyDerivationManager
    {
        byte[] DeriveKey(string password, byte[] salt = null);
    }

    public class KeyDerivationManager : IKeyDerivationManager
    {
        readonly IShelterVault _shelterVault;

        public KeyDerivationManager(IShelterVault shelterVault)
        {
            _shelterVault = shelterVault;
        }

        public byte[] DeriveKey(string password, byte[] salt = null)
        {
            ShelterVaultModel svm = _shelterVault.GetAllActiveVaults().FirstOrDefault();

            if (salt == null)
            {
                salt = RandomNumberGenerator.GetBytes(16);
            }

            IKeyDerivationService kds = svm.Version switch
            {
                (long)KeyDerivationAlgorithm.PBKDF2 => new PBKDF2KeyDerivationService(),
                (long)KeyDerivationAlgorithm.Argon2id => new Argon2KeyDerivationService(),
                _ => throw new NotSupportedException(),
            };

            KeyDerivationOptions kdo = svm.Version switch
            {
                (long)KeyDerivationAlgorithm.PBKDF2 => new KeyDerivationOptions
                {
                    Salt = salt,
                    Iterations = 10000,
                    KeyLength = 32,
                    Algorithm = KeyDerivationAlgorithm.PBKDF2,
                },
                (long)KeyDerivationAlgorithm.Argon2id => new KeyDerivationOptions
                {
                    Salt = salt,
                    Iterations = 3,
                    MemorySize = 64 * 1024,
                    Parallelism = Environment.ProcessorCount,
                    KeyLength = 32,
                    Algorithm = KeyDerivationAlgorithm.Argon2id,
                },
                _ => throw new NotSupportedException(),
            };

            return kds.DeriveKey(password, kdo);
        }
    }
}
