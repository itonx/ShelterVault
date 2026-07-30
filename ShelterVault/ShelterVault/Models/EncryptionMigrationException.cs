using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    public class EncryptionMigrationException : Exception
    {
        public EncryptionMigrationException()
            : base() { }

        public EncryptionMigrationException(string message)
            : base(message) { }
    }
}
