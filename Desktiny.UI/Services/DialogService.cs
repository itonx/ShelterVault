using Desktiny.UI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Desktiny.UI.Services
{
    public interface IDialogService
    {
        Task ShowConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = Constants.Global.DIALOG_TITLE_DEFAULT, string primaryButtonTextResourceKey = Constants.Global.DIALOG_CLOSE_DEFAULT);
        Task<bool> ShowContinueConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = Constants.Global.DIALOG_TITLE_DEFAULT, string primaryButtonTextResourceKey = Constants.Global.DIALOG_CLOSE_NO, string secondaryButtonResourceKey = Constants.Global.DIALOG_CLOSE_YES, ContentDialogResult expectedResult = ContentDialogResult.Primary);
    }
    public class DialogService : IDialogService
    {
        private readonly ILanguageService _languageService;
        private readonly IWindowService _windowService;

        public DialogService(ILanguageService languageService, IWindowService windowService)
        {
            _languageService = languageService;
            _windowService = windowService;
        }

        public async Task ShowConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = Constants.Global.DIALOG_TITLE_DEFAULT, string primaryButtonTextResourceKey = Constants.Global.DIALOG_CLOSE_DEFAULT)
        {
            ContentDialog dialog = BuildDialog(titleResourceKey, messageResourceKey, primaryButtonTextResourceKey);
            await dialog.ShowAsync();
        }

        public async Task<bool> ShowContinueConfirmationDialogAsync(string messageResourceKey, string titleResourceKey = Constants.Global.DIALOG_TITLE_DEFAULT, string primaryButtonTextResourceKey = Constants.Global.DIALOG_CLOSE_NO, string secondaryButtonResourceKey = Constants.Global.DIALOG_CLOSE_YES, ContentDialogResult expectedResult = ContentDialogResult.Primary)
        {
            ContentDialog dialog = BuildDialog(titleResourceKey, messageResourceKey, primaryButtonTextResourceKey);
            dialog.SecondaryButtonText = _languageService.GetLangValue(secondaryButtonResourceKey);
            ContentDialogResult result = await dialog.ShowAsync();
            return result == expectedResult;
        }

        private ContentDialog BuildDialog(string titleResourceKey, string messageResourceKey, string primaryButtonTextResourceKey = Constants.Global.DIALOG_CLOSE_DEFAULT)
        {
            string title = _languageService.GetLangValue(titleResourceKey);
            string message = _languageService.GetLangValue(messageResourceKey);
            string primaryButtonText = _languageService.GetLangValue(primaryButtonTextResourceKey);

            Window mainWindow = _windowService.GetMainWindow();
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = mainWindow.Content.XamlRoot;
            dialog.RequestedTheme = (mainWindow.Content as WinContainer).AppTheme.AppTheme;
            dialog.Title = title;
            dialog.PrimaryButtonText = primaryButtonText;
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new SimpleDialogView(message);

            return dialog;
        }
    }
}
