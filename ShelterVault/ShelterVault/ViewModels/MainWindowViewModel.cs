using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShelterVault.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isProgressBarVisible = false;

        private byte[] _masterKeyProtected;

        [RelayCommand]
        private void Loaded()
        {
            InitialSetUp();
        }

        [RelayCommand]
        private void ChangeTheme()
        {
            UITools.ChangeTheme();
        }

        internal byte[] GetMasterKeyUnprotected()
        {
            return ProtectedData.Unprotect(_masterKeyProtected, null, DataProtectionScope.CurrentUser);
        }

        internal void ProtectMasterKey(byte[] masterKey)
        {
            _masterKeyProtected = ProtectedData.Protect(masterKey, null, DataProtectionScope.CurrentUser);
        }

        private void InitialSetUp()
        {
            UITools.LoadAppIcon();
            UITools.LoadTheme();
            UITools.LoadInitialView();
        }
    }
}
