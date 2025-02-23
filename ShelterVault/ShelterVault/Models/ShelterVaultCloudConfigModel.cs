using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    public class ShelterVaultCloudConfigModel
    {
        public string Name { get; set; }
        public string EncryptedValues { get; set; }
        public string Iv { get; set; }

        public ShelterVaultCloudConfigModel()
        {
            
        }

        public ShelterVaultCloudConfigModel(string name, (byte[], byte[]) encyptedValues)
        {
            Name = name;
            EncryptedValues = encyptedValues.Item1.ToBase64();
            Iv = encyptedValues.Item2.ToBase64();
        }
    }
}
