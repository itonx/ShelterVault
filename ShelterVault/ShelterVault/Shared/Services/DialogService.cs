using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.Core.Shared.Interfaces;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Helpers;
using ShelterVault.Views;

namespace ShelterVault.Services
{
    public class DialogService : IDialogService
    {
        private readonly ILanguageService _languageService;

        public DialogService(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        public async Task ShowConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = LangResourceKeys.DIALOG_TITLE_DEFAULT, string primaryButtonTextResourceKey = LangResourceKeys.DIALOG_CLOSE_DEFAULT)
        {
            ContentDialog dialog = BuildDialog(titleResourceKey, messageResourceKey, primaryButtonTextResourceKey);
            await dialog.ShowAsync();
        }

        public async Task<bool> ShowContinueConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = LangResourceKeys.DIALOG_TITLE_DEFAULT, string primaryButtonTextResourceKey = LangResourceKeys.DIALOG_CLOSE_NO, string secondaryButtonResourceKey = LangResourceKeys.DIALOG_CLOSE_YES, int expectedResult = 1)
        {
            ContentDialog dialog = BuildDialog(titleResourceKey, messageResourceKey, primaryButtonTextResourceKey);
            dialog.SecondaryButtonText = _languageService.GetLangValue(secondaryButtonResourceKey);
            ContentDialogResult result = await dialog.ShowAsync();
            return result == (ContentDialogResult)expectedResult;
        }

        private ContentDialog BuildDialog(string titleResourceKey, string messageResourceKey, string primaryButtonTextResourceKey = LangResourceKeys.DIALOG_CLOSE_DEFAULT)
        {
            string title = _languageService.GetLangValue(titleResourceKey);
            string message = _languageService.GetLangValue(messageResourceKey);
            string primaryButtonText = _languageService.GetLangValue(primaryButtonTextResourceKey);

            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = WindowHelper.CurrentMainWindow.Content.XamlRoot;
            dialog.RequestedTheme = (WindowHelper.CurrentMainWindow.Content as FrameworkElement).RequestedTheme;
            dialog.Style = Application.Current.Resources[ShelterVaultConstants.DIALOG_STYLE_KEY] as Style;
            dialog.Title = title;
            dialog.PrimaryButtonText = primaryButtonText;
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new ShelterVaultMessageView(message);

            return dialog;
        }
    }
}
