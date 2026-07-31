using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.Interfaces;
using ShelterVault.Shared.Messages;

namespace ShelterVault.Services
{
    public class ProgressBarService : IProgressBarService
    {
        public async Task Show()
        {
            WeakReferenceMessenger.Default.Send(new ProgressBarRequestMessage(true));
            await Task.Delay(50);
        }

        public async Task Hide()
        {
            WeakReferenceMessenger.Default.Send(new ProgressBarRequestMessage(false));
            await Task.Delay(0);
        }
    }
}
