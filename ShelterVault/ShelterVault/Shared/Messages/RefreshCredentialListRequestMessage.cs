using CommunityToolkit.Mvvm.Messaging.Messages;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Messages
{
    public class RefreshCredentialListRequestMessage : ValueChangedMessage<bool>
    {
        public RefreshCredentialListRequestMessage(bool value) : base(value)
        {
        }
    }
}
