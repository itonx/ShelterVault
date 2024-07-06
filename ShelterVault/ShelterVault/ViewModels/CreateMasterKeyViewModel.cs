using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelterVault.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    public class CreateMasterKeyViewModel : ObservableObject
    {
        public IRelayCommand CreateMasterKeyCommand { get; }
        public IRelayCommand OnPasswordChangedCommand { get; }
        private bool _is8Characters;
        public bool Is8Characters
        {
            get => _is8Characters;
            set => SetProperty(ref _is8Characters, value);
        }
        private bool _hasLowercase;
        public bool HasLowercase
        {
            get => _hasLowercase;
            set => SetProperty(ref _hasLowercase, value);
        }
        private bool _hasUppercase;
        public bool HasUppercase
        {
            get => _hasUppercase;
            set => SetProperty(ref _hasUppercase, value);
        }
        private bool _hasNumber;
        public bool HasNumber
        {
            get => _hasNumber;
            set => SetProperty(ref _hasNumber, value);
        }
        private bool _hasSpecialChars;
        public bool HasSpecialChars
        {
            get => _hasSpecialChars;
            set => SetProperty(ref _hasSpecialChars, value);
        }
        private bool _isLessThan32Chars;
        public bool IsLessThan32Chars
        {
            get => _isLessThan32Chars;
            set => SetProperty(ref _isLessThan32Chars, value);
        }

        public CreateMasterKeyViewModel()
        {
            CreateMasterKeyCommand = new RelayCommand<Dictionary<string, StringBuilder>>(CreateMasterKey);
            OnPasswordChangedCommand = new RelayCommand<Dictionary<string, StringBuilder>>(OnPasswordChanged);
        }

        private void OnPasswordChanged(Dictionary<string, StringBuilder> passwords)
        {
            IsValidPassword(passwords.Values.First().ToString());
        }

        private async void CreateMasterKey(Dictionary<string, StringBuilder> masterKeyPasswords)
        {
            if (!masterKeyPasswords.Values.All(val => val.ToString().Equals(masterKeyPasswords.Values.First().ToString())))
            {
                await UITools.ShowConfirmationDialogAsync("Important", "Passwords don't match.");
                return;
            }
            if (!IsValidPassword(masterKeyPasswords.Values.First().ToString()))
            {
                await UITools.ShowConfirmationDialogAsync("Important", "Password doesn't meet minimum requirements.");
                return;
            }

            bool wasVaultCreated = ShelterVaultSqliteTool.CreateShelterVault(masterKeyPasswords.Values.First().ToString());
            if (wasVaultCreated) UITools.LoadMasterKeyConfirmationView();
        }

        private bool IsValidPassword(string password)
        {
            Is8Characters = password.Has8Characters();
            HasLowercase = password.HasLowercase();
            HasUppercase = password.HasUppercase();
            HasNumber = password.HasNumber();
            HasSpecialChars = password.HasSpecialChars();
            IsLessThan32Chars = password.IsLessThan32();

            return Is8Characters && HasLowercase && HasUppercase && HasNumber && HasSpecialChars && IsLessThan32Chars;
        }
    }
}
