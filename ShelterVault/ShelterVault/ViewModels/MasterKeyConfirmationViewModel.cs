using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelterVault.Tools;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    public partial class MasterKeyConfirmationViewModel : ObservableObject
    {
        [RelayCommand]
        private async Task ConfirmMasterKey(object parameter)
        {
            try
            {
                await UITools.ShowSpinner();
                if (ShelterVaultSqliteTool.IsMasterKeyValid(parameter?.ToString()))
                    UITools.LoadCredentialsView(Encoding.Unicode.GetBytes(parameter?.ToString()));
                else await UITools.ShowConfirmationDialogAsync("Important", "Wrong master key!");
            }
            finally
            {
                await UITools.HideSpinner();
            }
        }
    }
}
