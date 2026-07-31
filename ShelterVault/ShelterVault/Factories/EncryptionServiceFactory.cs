using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShelterVault.Interfaces;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;

namespace ShelterVault.Factories
{
    public class EncryptionServiceFactory : IEncryptionServiceFactory
    {
        public IEncryptionService Create(int version)
        {
            return version switch
            {
                (int)EncryptionVersion.v1 => new EncryptionServiceV0(),
                (int)EncryptionVersion.v2 => new EncryptionServiceV1(),
                _ => new EncryptionServiceV1(),
            };
        }
    }
}
