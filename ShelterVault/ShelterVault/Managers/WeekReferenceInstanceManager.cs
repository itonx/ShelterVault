using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Linq;

namespace ShelterVault.Managers
{
    public interface IWeakReferenceInstanceManager
    {
        public void AddInstance<T>(T instance, bool removeExisting = true);
        public void UnregisterInstances<T>();
    }

    public class WeakReferenceInstanceManager : IWeakReferenceInstanceManager
    {
        private readonly List<object> _instances = new List<object>();

        public void AddInstance<T>(T instance, bool removeExisting = true)
        {
            if (removeExisting)
            {
                UnregisterInstances<T>();
            }
            _instances.Add(instance);
        }

        public void UnregisterInstances<T>()
        {
            List<object> tmpList = new List<object>();

            foreach (var instance in _instances.Where(instance => instance is T))
            {
                WeakReferenceMessenger.Default.UnregisterAll(instance);
                tmpList.Add(instance);
            }

            foreach (var instance in tmpList)
            {
                _instances.Remove(instance);
            }
        }
    }
}
