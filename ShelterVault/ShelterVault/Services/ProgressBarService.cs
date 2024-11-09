using CommunityToolkit.Mvvm.Messaging;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.Services
{
    internal interface IProgressBarService
    {
        Task Show();
        Task Hide();
    }
    internal class ProgressBarService : IProgressBarService
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
