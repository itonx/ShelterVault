﻿using CommunityToolkit.Mvvm.Messaging.Messages;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.Shared.Messages
{
    public class CurrentAppStateRequestMessage : ValueChangedMessage<ShelterVaultAppState>
    {
        public CurrentAppStateRequestMessage(ShelterVaultAppState value) : base(value)
        {
        }
    }
}