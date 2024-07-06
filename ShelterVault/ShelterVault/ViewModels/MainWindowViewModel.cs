using CommunityToolkit.Mvvm.Input;
using ShelterVault.Tools;
using ShelterVault.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace ShelterVault.ViewModels
{
    public class MainWindowViewModel
    {
        private byte[] _masterKeyProtected;
        public IRelayCommand OnLoadedCommand { get; }

        public MainWindowViewModel()
        {
            OnLoadedCommand = new RelayCommand(OnLoaded);
        }

        internal byte[] GetMasterKeyUnprotected()
        {
            return ProtectedData.Unprotect(_masterKeyProtected, null, DataProtectionScope.CurrentUser);
        }

        internal void ProtectMasterKey(byte[] masterKey)
        {
            _masterKeyProtected = ProtectedData.Protect(masterKey, null, DataProtectionScope.CurrentUser);
        }

        private void OnLoaded()
        {
            InitialSetUp();
        }

        private void InitialSetUp()
        {
            UITools.LoadAppIcon();
            UITools.LoadInitialView();
        }
    }
}
