using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelterVault.Managers;
using ShelterVault.Models;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Extensions;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    internal partial class PasswordConfirmationViewModel : ObservableObject
    {
        private readonly IDialogManager _dialogManager;
        private readonly ILanguageService _languageService;

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

        public PasswordConfirmationViewModel(IDialogManager dialogManager, ILanguageService languageService)
        {
            _dialogManager = dialogManager;
            _languageService = languageService;
            HeaderText = _languageService.GetLangValue(LangResourceKeys.MASTER_KEY_MUST);
        }

        [RelayCommand]
        private void PasswordChanged(string password)
        {
            IsValidPassword(password);
        }

        public async Task<bool> AreCredentialsValid(Credentials credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.Title))
            {
                await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_EMPTY_TITLE);
                return false;
            }

            return await ArePasswordsValid(credentials.Password, credentials.PasswordConfirmation);
        }

        public async Task<bool> ArePasswordsValid(string password, string passwordConfirmation)
        {
            if (!IsValidPassword(password))
            {
                await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_PASSWORD_MINIMUM_REQUIREMENTS_ERROR);
                return false;
            }
            else if (password != passwordConfirmation)
            {
                await _dialogManager.ShowConfirmationDialogAsync(LangResourceKeys.DIALOG_CREDENTIALS_PASSWORD_DO_NOT_MATCH);
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
