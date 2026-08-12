using System;
using System.Threading.Tasks;
using Desktiny.WinUI;
using Desktiny.WinUI.Services;
using Desktiny.WinUI.Tools;
using Desktiny.WinUI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualBasic;
using ShelterVault.Interfaces;
using ShelterVault.ViewModels;
using ShelterVault.Views;

namespace ShelterVault.Managers
{
    public class DialogManager : IDialogManager
    {
        private readonly IShelterVaultStateService _shelterVaultStateService;
        private readonly IDialogLangService _dialogService;

        public DialogManager(
            IShelterVaultStateService shelterVaultStateService,
            IDialogLangService dialogService
        )
        {
            _shelterVaultStateService = shelterVaultStateService;
            _dialogService = dialogService;
        }

        public async Task ShowConfirmationDialogAsync(
            string messageResourceKey,
            string titleResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_TITLE_DEFAULT,
            string primaryButtonTextResourceKey =
                Desktiny.WinUI.Constants.Global.DIALOG_CLOSE_DEFAULT
        )
        {
            _shelterVaultStateService.SetDialogStatus(true);
            await _dialogService.ShowInformationAsync(
                messageResourceKey,
                titleResourceKey,
                primaryButtonTextResourceKey
            );
            _shelterVaultStateService.SetDialogStatus(false);
        }

        public async Task<bool> ShowContinueConfirmationDialogAsync(
            string messageResourceKey,
            string titleResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_TITLE_DEFAULT,
            string primaryButtonTextResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_CLOSE_NO,
            string secondaryButtonResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_CLOSE_YES,
            ContentDialogResult expectedResult = ContentDialogResult.Primary
        )
        {
            _shelterVaultStateService.SetDialogStatus(true);
            bool result = await _dialogService.ShowYesNoAsync(
                messageResourceKey,
                titleResourceKey,
                primaryButtonTextResourceKey,
                secondaryButtonResourceKey,
                expectedResult
            );
            _shelterVaultStateService.SetDialogStatus(false);
            return result;
        }

        public async Task<string> ShowNewVaultMigrationDialog(
            string titleResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_TITLE_DEFAULT,
            string primaryButtonTextResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_CLOSE_NO,
            string secondaryButtonResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_CLOSE_YES,
            ContentDialogResult expectedResult = ContentDialogResult.Secondary
        )
        {
            _shelterVaultStateService.SetDialogStatus(true);
            ContentDialog dialog = BuildDialog(
                titleResourceKey,
                primaryButtonTextResourceKey,
                secondaryButtonResourceKey
            );

            var createMasterKeyPage = dialog.Content as CreateMasterKeyPage;
            ContentDialogResult result;
            string mk = "";

            do
            {
                result = await dialog.ShowAsync();
                if (
                    result == expectedResult
                    && createMasterKeyPage.DataContext is CreateMasterKeyViewModel vm
                )
                {
                    if (
                        await vm.PasswordRequirementsVM.ArePasswordsValid(
                            vm.Password,
                            vm.PasswordConfirmation
                        )
                    )
                    {
                        mk = vm.Password;
                    }
                }
            } while (result == expectedResult && string.IsNullOrWhiteSpace(mk));

            return result == expectedResult ? mk : "";
        }

        private ContentDialog BuildDialog(
            string titleResourceKey,
            string primaryButtonTextResourceKey,
            string secondaryButtonResourceKey
        )
        {
            string title = LangService.GetLangValue(titleResourceKey);
            string primaryButtonText = LangService.GetLangValue(primaryButtonTextResourceKey);

            Window mainWindow = WindowHelper.CurrentMainWindow;
            ContentDialog dialog = new ContentDialog();

            dialog.XamlRoot = mainWindow.Content.XamlRoot;
            dialog.RequestedTheme = (mainWindow.Content as Winston).AppTheme.ElementTheme;
            dialog.Title = title;
            dialog.PrimaryButtonText = primaryButtonText;
            dialog.SecondaryButtonText = LangService.GetLangValue(secondaryButtonResourceKey);
            dialog.DefaultButton = ContentDialogButton.Primary;
            var page = new CreateMasterKeyPage();
            page.SetDialogMode();
            page.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.Transparent
            );
            dialog.Content = page;
            dialog.Resources["ContentDialogMaxWidth"] = 700;

            return dialog;
        }
    }
}
