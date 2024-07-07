using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelterVault.Models;
using ShelterVault.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    public class PasswordConfirmationViewModel : ObservableObject
    {
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
        private string _headerText;
        public string HeaderText
        {
            get => _headerText;
            set => SetProperty(ref _headerText, value);
        }

        public PasswordConfirmationViewModel()
        {
            OnPasswordChangedCommand = new RelayCommand<string>(OnPasswordChanged);
            HeaderText = "Master key password must:";
        }

        public async Task<bool> AreCredentialsValid(Credential credential)
        {
            if (string.IsNullOrWhiteSpace(credential.Title)) 
            {
                await UITools.ShowConfirmationDialogAsync("Important", "Title can't be empty");
                return false;
            }

            return await ArePasswordsValid(credential.Password, credential.PasswordConfirmation);
        }

        public async Task<bool> ArePasswordsValid(string password, string passwordConfirmation)
        {
            if (!IsValidPassword(password))
            {
                await UITools.ShowConfirmationDialogAsync("Important", "Password doesn't meet minimum requirements.");
                return false;
            }
            else if (password != passwordConfirmation)
            {
                await UITools.ShowConfirmationDialogAsync("Important", "Passwords don't match.");
                return false;
            }

            return true;
        }

        private void OnPasswordChanged(string password)
        {
            IsValidPassword(password);
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
