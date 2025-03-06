namespace ShelterVault.Core.Shared.Interfaces
{
    public interface IUIThreadService
    {
        void Execute(Action rutine);
    }
}
