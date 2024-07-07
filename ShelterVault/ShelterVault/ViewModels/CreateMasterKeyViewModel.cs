using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelterVault.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShelterVault.ViewModels
{
    public class CreateMasterKeyViewModel : ObservableObject
    {
        public IRelayCommand CreateMasterKeyCommand { get; }
        private PasswordConfirmationViewModel _passwordRequirementsVM;
        public PasswordConfirmationViewModel PasswordRequirementsVM
        {
            get => _passwordRequirementsVM;
            set => SetProperty(ref _passwordRequirementsVM, value);
        }

        public CreateMasterKeyViewModel()
        {
            CreateMasterKeyCommand = new RelayCommand<Dictionary<string, StringBuilder>>(CreateMasterKey);
            PasswordRequirementsVM = new PasswordConfirmationViewModel();
        }

        private async void CreateMasterKey(Dictionary<string, StringBuilder> masterKeyPasswords)
        {
            if (await PasswordRequirementsVM.ArePasswordsValid(masterKeyPasswords.Values.First().ToString(), masterKeyPasswords.Values.Last().ToString()))
            {
                bool wasVaultCreated = ShelterVaultSqliteTool.CreateShelterVault(masterKeyPasswords.Values.First().ToString());
                if (wasVaultCreated) UITools.LoadMasterKeyConfirmationView();
            }
        }
    }
}
