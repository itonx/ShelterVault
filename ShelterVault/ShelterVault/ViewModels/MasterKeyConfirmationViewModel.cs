using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ShelterVault.Tools;
using System.Reflection.Metadata;
using System.Text;

namespace ShelterVault.ViewModels
{
    public class MasterKeyConfirmationViewModel
    {
        public IRelayCommand ConfirmMasterKeyCommand { get; }

        public MasterKeyConfirmationViewModel()
        {
            ConfirmMasterKeyCommand = new RelayCommand<object>(ConfirmMasterKey);
        }

        private void ConfirmMasterKey(object parameter)
        {
            if(parameter is KeyRoutedEventArgs key && key.Key == Windows.System.VirtualKey.Enter && key.OriginalSource is PasswordBox pb && ShelterVaultSqliteTool.IsMasterKeyValid(pb.Password))
                UITools.LoadCredentialsView(Encoding.Unicode.GetBytes(pb.Password));
        }
    }
}
