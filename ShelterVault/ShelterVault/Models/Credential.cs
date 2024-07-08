using CommunityToolkit.Mvvm.ComponentModel;
using ShelterVault.Tools;
using System;
using System.Text;

namespace ShelterVault.Models
{
    public class Credential : ObservableObject
    {
        public string UUID { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConfirmation { get; set; }
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
                Password == item.Password && PasswordConfirmation == item.PasswordConfirmation &&
                EncryptedPassword == item.EncryptedPassword && InitializationVector == item.InitializationVector && 
                Url == item.Url && Notes == item.Notes;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UUID, Title, Username, EncryptedPassword, InitializationVector, Url, Notes);
        }

        public bool IsUpdatedCredentialValid(StringBuilder err)
        {
            if (err == null) throw new ArgumentNullException("Error while validating the new credential.");
            if (string.IsNullOrWhiteSpace(Title)) err.AppendLine("Title can't be empty");

            return err.Length == 0;
        }

        public Credential GetUpdatedCredentialValues((byte[], byte[]) encryptedValues)
        {
            Credential newCredential = this.Clone();
            newCredential.EncryptedPassword = Convert.ToBase64String(encryptedValues.Item1);
            newCredential.InitializationVector = Convert.ToBase64String(encryptedValues.Item2);

            return newCredential;
        }
    }
}
