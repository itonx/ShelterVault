using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Messages
{
    internal class CheckSelectedCredentialsAfterSyncMessage : ValueChangedMessage<bool>
    {
        public CheckSelectedCredentialsAfterSyncMessage(bool value) : base(value)
        {
        }
    }
}
