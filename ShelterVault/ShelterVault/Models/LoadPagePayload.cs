using System;
using System.Collections.Generic;
using System.Text;
using ShelterVault.Shared.Enums;

namespace ShelterVault.Models
{
    public class LoadPagePayload
    {
        public ShelterVaultPage ShelterVaultPage { get; private set; }
        public object Param { get; private set; }

        private LoadPagePayload(ShelterVaultPage shelterVaultPage, object param)
        {
            ShelterVaultPage = shelterVaultPage;
            Param = param;
        }

        private LoadPagePayload(ShelterVaultPage shelterVaultPage)
        {
            ShelterVaultPage = shelterVaultPage;
            Param = null;
        }

        public static LoadPagePayload Create(ShelterVaultPage shelterVaultPage)
        {
            return new LoadPagePayload(shelterVaultPage);
        }

        public static LoadPagePayload Create(ShelterVaultPage shelterVaultPage, object param)
        {
            return new LoadPagePayload(shelterVaultPage, param);
        }
    }
}
