using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    public sealed class PBKDF2KeyDerivation
    {
        public byte[] DeriveKey(string password, KeyDerivationOptions options)
        {
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(options);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                options.Salt,
                options.Iterations,
                HashAlgorithmName.SHA256
            );

            return pbkdf2.GetBytes(options.KeyLength);
        }
    }
}
