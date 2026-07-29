using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ShelterVault.Models;

namespace ShelterVault.Services
{
    public interface IKeyDerivationService
    {
        byte[] DeriveKey(string password, KeyDerivationOptions options);
    }

    public sealed class PBKDF2KeyDerivationService : IKeyDerivationService
    {
        public byte[] DeriveKey(string password, KeyDerivationOptions options)
        {
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
