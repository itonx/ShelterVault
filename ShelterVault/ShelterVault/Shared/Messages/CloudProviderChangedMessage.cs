using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Messages
{
    public class CloudProviderChangedMessage : ValueChangedMessage<bool>
    {
        public CloudProviderChangedMessage(bool value) : base(value)
        {
        }
    }
}
