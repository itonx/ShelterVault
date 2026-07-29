using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    public enum KeyDerivationAlgorithm
    {
        PBKDF2 = 1,
        Argon2id = 2,
    }

    public sealed class KeyDerivationOptions
    {
        public required byte[] Salt { get; init; }

        public required KeyDerivationAlgorithm Algorithm { get; init; }

        public int Iterations { get; init; }

        public int MemorySize { get; init; }

        public int Parallelism { get; init; }

        public int KeyLength { get; init; } = 32;
    }
}
