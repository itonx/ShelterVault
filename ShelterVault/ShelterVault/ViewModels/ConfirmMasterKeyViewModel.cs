using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.DataLayer;
using ShelterVault.Services;
using ShelterVault.Shared.Messages;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.ViewModels
{
    public partial class ConfirmMasterKeyViewModel : ObservableObject
    {
        private readonly IMasterKeyService _masterKeyService;
        private readonly IDialogService _dialogService;
        private readonly IProgressBarService _progressBarService;
        private readonly IShelterVaultLocalStorage _shelterVaultLocalStorage;

        public ConfirmMasterKeyViewModel(IMasterKeyService masterKeyService, IDialogService dialogService, IProgressBarService progressBarService, IShelterVaultLocalStorage shelterVaultLocalStorage)
        {
            _masterKeyService = masterKeyService;
            _dialogService = dialogService;
            _progressBarService = progressBarService;
            _shelterVaultLocalStorage = shelterVaultLocalStorage;
        }

        [RelayCommand]
        private async Task ConfirmMasterKey(object parameter)
        {
            try
            {
                await _progressBarService.Show();
                if (_shelterVaultLocalStorage.IsMasterKeyValid(parameter?.ToString()))
                {
                    string salt = _shelterVaultLocalStorage.GetMasterKeySalt();
                    _masterKeyService.ProtectMasterKey(Encoding.Unicode.GetBytes(parameter?.ToString()), Encoding.Unicode.GetBytes(salt));
                    WeakReferenceMessenger.Default.Send(new CurrentAppStateRequestMessage(Shared.Enums.ShelterVaultAppState.NavigationView));
                }
                else await _dialogService.ShowConfirmationDialogAsync("Important", "Wrong master key!");
            }
            finally
            {
                await _progressBarService.Hide();
            }
        }
    }
}
