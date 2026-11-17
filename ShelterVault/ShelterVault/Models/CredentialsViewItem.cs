namespace ShelterVault.Models
{
    public class CredentialsViewItem : ShelterVaultCredentialsModel
    {
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool SkipPageLoader { get; set; } = false;

        public CredentialsViewItem(
            ShelterVaultCredentialsModel shelterVaultCredentialsModel,
            Credentials decryptedCredentials
        )
            : base(shelterVaultCredentialsModel)
        {
            this.Title = decryptedCredentials.Title;
            this.Username = decryptedCredentials.Username;
            this.Password = decryptedCredentials.Password;
        }

        public CredentialsViewItem Clone() => (CredentialsViewItem)this.MemberwiseClone();
    }
}
