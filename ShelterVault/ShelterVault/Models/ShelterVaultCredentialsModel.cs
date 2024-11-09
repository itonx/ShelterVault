using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    internal class ShelterVaultCredentialsModel
    {
        public string UUID { get; set; }
        public string EncryptedValues { get; set; }
        public string Iv { get; set; }

        public ShelterVaultCredentialsModel()
        {
            
        }

        public ShelterVaultCredentialsModel((byte[], byte[]) encyptedValues)
        {
            UUID = Guid.NewGuid().ToString();
            EncryptedValues = encyptedValues.Item1.ToBase64();
            Iv = encyptedValues.Item2.ToBase64();
        }

        public ShelterVaultCredentialsModel(Credentials credentials, (byte[], byte[]) encyptedValues)
        {
            UUID = credentials.UUID;
            EncryptedValues = encyptedValues.Item1.ToBase64();
            Iv = encyptedValues.Item2.ToBase64();
        }
    }
}
