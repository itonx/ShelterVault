using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace ShelterVault.Shared.Extensions
{
    public static class BehaviorExtensions
    {
        public static TDependencyObject GetDependencyObjectFromBehavior<TDependencyObject>(this DependencyObject dependencyObject) where TDependencyObject : class
        {
            if (dependencyObject is Behavior behavior) return behavior.AssociatedObject as TDependencyObject;
            return dependencyObject as TDependencyObject;
        }
    }
}
