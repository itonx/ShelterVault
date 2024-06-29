using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    public class Credential
    {
        public string UUID { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public string InitializationVector { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }
    }
}
