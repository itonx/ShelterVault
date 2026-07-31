namespace ShelterVault.Interfaces
{
    public interface IWeakReferenceInstanceManager
    {
        public void AddInstance<T>(T instance, bool removeExisting = true);
        public void UnregisterInstances<T>();
    }
}
