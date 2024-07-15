using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelterVault.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    public partial class CreateMasterKeyViewModel : ObservableObject
    {
        [ObservableProperty]
        private PasswordConfirmationViewModel _passwordRequirementsVM = new PasswordConfirmationViewModel();

        [RelayCommand]
        private async Task CreateMasterKey(Dictionary<string, StringBuilder> masterKeyPasswords)
        {
            try
            {
                await UITools.ShowSpinner();
                if (await PasswordRequirementsVM.ArePasswordsValid(masterKeyPasswords.Values.First().ToString(), masterKeyPasswords.Values.Last().ToString()))
                {
                    bool wasVaultCreated = ShelterVaultSqliteTool.CreateShelterVault(masterKeyPasswords.Values.First().ToString());
                    if (wasVaultCreated) UITools.LoadMasterKeyConfirmationView();
                }
            }
            finally
            {
                await UITools.HideSpinner();
            }
        }
    }
}
