﻿using CommunityToolkit.Mvvm.Input;
using ShelterVault.Tools;
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
            if(ShelterVaultSqliteTool.IsMasterKeyValid(parameter?.ToString()))
                UITools.LoadCredentialsView(Encoding.Unicode.GetBytes(parameter?.ToString()));
        }
    }
}
