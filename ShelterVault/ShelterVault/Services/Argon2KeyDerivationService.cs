using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;
using ShelterVault.Models;

namespace ShelterVault.Services
{
    public sealed class Argon2KeyDerivationService : IKeyDerivationService
    {
        public byte[] DeriveKey(string password, KeyDerivationOptions options)
        {
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(options);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = options.Salt,
                Iterations = options.Iterations,
                MemorySize = options.MemorySize,
                DegreeOfParallelism = options.Parallelism,
            };

            return argon2.GetBytes(options.KeyLength);
        }
    }
}
