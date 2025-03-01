using Microsoft.UI.Xaml.Controls;
using ShelterVault.Services;
using ShelterVault.Shared.Constants;
using System.Threading.Tasks;

namespace ShelterVault.Managers
{
    public interface IDialogManager
    {
        Task ShowConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = LangResourceKeys.DIALOG_TITLE_DEFAULT, string primaryButtonTextResourceKey = LangResourceKeys.DIALOG_CLOSE_DEFAULT);
        Task<bool> ShowContinueConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = LangResourceKeys.DIALOG_TITLE_DEFAULT, string primaryButtonTextResourceKey = LangResourceKeys.DIALOG_CLOSE_NO, string secondaryButtonResourceKey = LangResourceKeys.DIALOG_CLOSE_YES, ContentDialogResult expectedResult = ContentDialogResult.Primary);
    }

    public class DialogManager : IDialogManager
    {
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly IDialogService _dialogService;

        public DialogManager(IShelterVaultStateService shelterVaultStateService, IDialogService dialogService)
        {
            _shelterVaultStateService = shelterVaultStateService;
            _dialogService = dialogService;
        }

        public async Task ShowConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = "DialogTitleDefault/Text", string primaryButtonTextResourceKey = "DialogCloseDefault/Text")
        {
            _shelterVaultStateService.SetDialogStatus(true);
            await _dialogService.ShowConfirmationDialogAsync(messageResourceKey, titleResourceKey, primaryButtonTextResourceKey);
            _shelterVaultStateService.SetDialogStatus(false);
        }

        public async Task<bool> ShowContinueConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = "DialogTitleDefault/Text", string primaryButtonTextResourceKey = "DialogCloseNo/Text", string secondaryButtonResourceKey = "DialogCloseYes/Text", ContentDialogResult expectedResult = ContentDialogResult.Primary)
        {
            _shelterVaultStateService.SetDialogStatus(true);
            bool result = await _dialogService.ShowContinueConfirmationDialogAsync(messageResourceKey, titleResourceKey, primaryButtonTextResourceKey, secondaryButtonResourceKey, expectedResult);
            _shelterVaultStateService.SetDialogStatus(false);
            return result;
        }
    }
}
