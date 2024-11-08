using CommunityToolkit.Mvvm.ComponentModel;
using ShelterVault.Shared.Extensions;
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
        public string Iv { get; set; }
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
                EncryptedPassword == item.EncryptedPassword && Iv == item.Iv && 
                Url == item.Url && Notes == item.Notes;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UUID, Title, Username, EncryptedPassword, Iv, Url, Notes);
        }

        public bool IsUpdatedCredentialValid(StringBuilder err)
        {
            if (err == null) throw new ArgumentNullException("Error while validating the new credential.");
            if (string.IsNullOrWhiteSpace(Title)) err.AppendLine("Title can't be empty");

            return err.Length == 0;
        }

        public Credential GenerateBase64EncryptedValues((byte[], byte[]) encryptedValues)
        {
            Credential newCredential = this.Clone();
            newCredential.EncryptedPassword = encryptedValues.Item1.ToBase64();
            newCredential.Iv = encryptedValues.Item2.ToBase64();

            return newCredential;
        }
    }
}
