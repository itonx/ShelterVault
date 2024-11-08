﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.ViewModels
{
    public partial class PasswordConfirmationViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        [ObservableProperty]
        private bool _is8Characters;
        [ObservableProperty]
        private bool _hasLowercase;
        [ObservableProperty]
        private bool _hasUppercase;
        [ObservableProperty]
        private bool _hasNumber;
        [ObservableProperty]
        private bool _hasSpecialChars;
        [ObservableProperty]
        private bool _isLessThan32Chars;
        [ObservableProperty]
        private string _headerText = "Master key password must:";

        public PasswordConfirmationViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        [RelayCommand]
        private void PasswordChanged(string password)
        {
            IsValidPassword(password);
        }

        public async Task<bool> AreCredentialsValid(Credential credential)
        {
            if (string.IsNullOrWhiteSpace(credential.Title)) 
            {
                await _dialogService.ShowConfirmationDialogAsync("Important", "Title can't be empty");
                return false;
            }

            return await ArePasswordsValid(credential.Password, credential.PasswordConfirmation);
        }

        public async Task<bool> ArePasswordsValid(string password, string passwordConfirmation)
        {
            if (!IsValidPassword(password))
            {
                await _dialogService.ShowConfirmationDialogAsync("Important", "Password doesn't meet minimum requirements.");
                return false;
            }
            else if (password != passwordConfirmation)
            {
                await _dialogService.ShowConfirmationDialogAsync("Important", "Passwords don't match.");
                return false;
            }

            return true;
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
