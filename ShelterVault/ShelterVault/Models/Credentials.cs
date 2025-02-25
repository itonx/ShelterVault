using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ShelterVault.Models
{
    public class Credentials : ObservableObject
    {
        public string UUID { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
        public string Iv { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }
        public string ShelterVaultUuid { get; set; }
        public long Version { get; set; }

        public Credentials()
        {
            UUID = string.Empty;
            Title = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            PasswordConfirmation = string.Empty;
            Iv = string.Empty;
            Url = string.Empty;
            Notes = string.Empty;
            ShelterVaultUuid = string.Empty;
            Version = 0;
        }

        public Credentials(string shelterVaultUuid)
        {
            ShelterVaultUuid = shelterVaultUuid;
        }

        public Credentials(string jsonValues, ShelterVaultCredentialsModel shelterVaultCredentialsModel)
        {
            Credentials credentials = GetCredentialFrom(jsonValues);
            this.UUID = shelterVaultCredentialsModel.UUID;
            this.Title = credentials.Title;
            this.Username = credentials.Username;
            this.Password = this.PasswordConfirmation = credentials.Password;
            this.Iv = shelterVaultCredentialsModel.Iv;
            this.Url = credentials.Url;
            this.Notes = credentials.Notes;
            this.ShelterVaultUuid = shelterVaultCredentialsModel.ShelterVaultUuid;
            this.Version = shelterVaultCredentialsModel.Version;
        }

        public Credentials Clone() => (Credentials)this.MemberwiseClone();

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Credentials item = (Credentials)obj;
            return UUID == item.UUID && Title == item.Title && Username == item.Username &&
                Password == item.Password && PasswordConfirmation == item.PasswordConfirmation &&
                Iv == item.Iv && Url == item.Url && Notes == item.Notes && Version == item.Version;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UUID, Title, Username, Iv, Url, Notes, Version);
        }

        public string GetJsonValues()
        {
            object values = new { Title, Username, Password, Url, Notes };
            return System.Text.Json.JsonSerializer.Serialize(values);
        }

        public static Credentials GetCredentialFrom(string jsonValues)
        {
            return System.Text.Json.JsonSerializer.Deserialize<Credentials>(jsonValues);
        }
    }
}
