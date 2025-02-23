﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Models
{
    public class CredentialsViewItem : ShelterVaultCredentialsModel
    {
        public string Title { get; set; }
        public bool SkipPageLoader { get; set; } = false;

        public CredentialsViewItem(ShelterVaultCredentialsModel shelterVaultCredentialsModel, Credentials decryptedCredentials) : base(shelterVaultCredentialsModel)
        {
            this.Title = decryptedCredentials.Title;
        }

        public CredentialsViewItem Clone() => (CredentialsViewItem)this.MemberwiseClone();
    }
}
