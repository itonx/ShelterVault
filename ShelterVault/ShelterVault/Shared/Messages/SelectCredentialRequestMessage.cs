using CommunityToolkit.Mvvm.Messaging.Messages;
using ShelterVault.Models;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Messages
{
    public class SelectCredentialRequestMessage : ValueChangedMessage<Credential>
    {
        public SelectCredentialRequestMessage(Credential value) : base(value)
        {
        }
    }
}
