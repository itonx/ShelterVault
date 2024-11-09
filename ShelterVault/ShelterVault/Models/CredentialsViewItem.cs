using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    internal class CredentialsViewItem : ShelterVaultCredentialsModel
    {
        public string Title { get; set; }

        public CredentialsViewItem(ShelterVaultCredentialsModel shelterVaultCredentialsModel, Credentials decryptedCredentials) : base(shelterVaultCredentialsModel)
        {
            this.Title = decryptedCredentials.Title;
        }

        public CredentialsViewItem Clone() => (CredentialsViewItem)this.MemberwiseClone();
    }
}
