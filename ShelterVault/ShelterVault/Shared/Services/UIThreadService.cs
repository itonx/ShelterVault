using ShelterVault.Core.Shared.Interfaces;

namespace ShelterVault.Shared.Services
{
    public class UIThreadService : IUIThreadService
    {
        public void Execute(Action rutine)
        {
            AppDispatcher.UIThreadDispatcher?.TryEnqueue(() =>
            {
                rutine.Invoke();
            });
        }
    }
}
