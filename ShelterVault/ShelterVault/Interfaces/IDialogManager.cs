using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace ShelterVault.Interfaces
{
    public interface IDialogManager
    {
        Task ShowConfirmationDialogAsync(
            string messageResourceKey,
            string titleResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_TITLE_DEFAULT,
            string primaryButtonTextResourceKey =
                Desktiny.WinUI.Constants.Global.DIALOG_CLOSE_DEFAULT
        );
        Task<bool> ShowContinueConfirmationDialogAsync(
            string messageResourceKey,
            string titleResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_TITLE_DEFAULT,
            string primaryButtonTextResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_CLOSE_NO,
            string secondaryButtonResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_CLOSE_YES,
            ContentDialogResult expectedResult = ContentDialogResult.Primary
        );

        Task<string> ShowNewVaultMigrationDialog(
            string titleResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_TITLE_DEFAULT,
            string primaryButtonTextResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_CLOSE_NO,
            string secondaryButtonResourceKey = Desktiny.WinUI.Constants.Global.DIALOG_CLOSE_YES,
            ContentDialogResult expectedResult = ContentDialogResult.Secondary
        );
    }
}
