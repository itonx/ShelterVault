using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    internal class ShelterVaultModel
    {
        public string Name { get; set; }
        public string MasterKeyHash { get; set; }
        public string Iv { get; set; }
        public string Salt { get; set; }
    }
}
