using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ShelterVault.Models
{
    public class Credential : ObservableObject
    {
        public string UUID { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public string InitializationVector { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }

        public Credential Clone() => (Credential)this.MemberwiseClone();

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Credential item = (Credential)obj;
            return UUID == item.UUID && Title == item.Title && Username == item.Username && 
                EncryptedPassword == item.EncryptedPassword && InitializationVector == item.InitializationVector && 
                Url == item.Url && Notes == item.Notes;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UUID, Title, Username, EncryptedPassword, InitializationVector, Url, Notes);
        }
    }
}
