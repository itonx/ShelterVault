using Microsoft.Azure.Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Services
{
    public interface IUIThreadService
    {
        void Execute(Action rutine);
    }

    public class UIThreadService : IUIThreadService
    {
        public void Execute(Action rutine)
        {
            AppDispatcher.UIThreadDispatcher?.TryEnqueue(() => {
                rutine.Invoke();
            });
        }
    }
}
