using ShelterVault.Shared.Constants;

namespace ShelterVault.Core.Shared.Interfaces
{
    public interface IDialogService
    {
        Task ShowConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = LangResourceKeys.DIALOG_TITLE_DEFAULT, string primaryButtonTextResourceKey = LangResourceKeys.DIALOG_CLOSE_DEFAULT);
        Task<bool> ShowContinueConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = LangResourceKeys.DIALOG_TITLE_DEFAULT, string primaryButtonTextResourceKey = LangResourceKeys.DIALOG_CLOSE_NO, string secondaryButtonResourceKey = LangResourceKeys.DIALOG_CLOSE_YES, int expectedResult = 1);
    }
}
