using System.Threading.Tasks;
using Desktiny.WinUI.Services;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.Interfaces;

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
    }
}
