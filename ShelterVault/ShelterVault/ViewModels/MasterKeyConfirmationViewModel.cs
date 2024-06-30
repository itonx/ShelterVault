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
            ConfirmMasterKeyCommand = new RelayCommand<string>(ConfirmMasterKey);
        }

        private void ConfirmMasterKey(string masterKey)
        {
            if (ShelterVaultSqliteTool.IsMasterKeyValid(masterKey)) UITools.LoadCredentialsView(Encoding.Unicode.GetBytes(masterKey));
        }
    }
}
