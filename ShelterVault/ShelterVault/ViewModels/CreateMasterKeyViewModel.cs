using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.Models;
using ShelterVault.Tools;
using ShelterVault.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    public class CreateMasterKeyViewModel : ObservableObject
    {
        private readonly Regex _passwordChecker = new Regex(@"^(?=.*[0-9])(?=.*[!@#$%^&*(),.?"":{}|<>])[a-zA-Z0-9!@#$%^&*(),.?"":{}|<>]{8,32}$");

        public IRelayCommand CreateMasterKeyCommand { get; }

        public CreateMasterKeyViewModel()
        {
            CreateMasterKeyCommand = new RelayCommand<Dictionary<string, StringBuilder>>(CreateMasterKey);
        }

        private async void CreateMasterKey(Dictionary<string, StringBuilder> masterKeyPasswords)
        {
            if (!await IsValid(masterKeyPasswords)) return;
            bool wasVaultCreated = ShelterVaultSqliteTool.CreateShelterVault(masterKeyPasswords.Values.First().ToString());
            if (wasVaultCreated) UITools.LoadMasterKeyConfirmationView(Encoding.Unicode.GetBytes(masterKeyPasswords.Values.First().ToString()));
        }

        private async Task<bool> IsValid(Dictionary<string, StringBuilder> passwords)
        {
            StringBuilder err = new StringBuilder();

            if (!passwords.Values.All(val => val.ToString().Equals(passwords.Values.First().ToString()))) err.AppendLine("[-] Passwords don't match");
            if (!_passwordChecker.IsMatch(passwords.Values.First().ToString())) err.AppendLine("[-] Password doesn't meet minimum requirements.");

            if (err.Length > 0)
            {
                await UITools.ShowConfirmationDialogAsync("Important", err.ToString());
                return false;
            }

            return true;
        }
    }
}
