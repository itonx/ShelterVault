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

        public CredentialsViewItem(ShelterVaultCredentialsModel shelterVaultCredentialsModel, Credentials credentials)
        {
            this.UUID = shelterVaultCredentialsModel.UUID;
            this.EncryptedValues = shelterVaultCredentialsModel.EncryptedValues;
            this.Iv = shelterVaultCredentialsModel.Iv;
            this.Title = credentials.Title;
        }

        public CredentialsViewItem Clone() => (CredentialsViewItem)this.MemberwiseClone();
    }
}
