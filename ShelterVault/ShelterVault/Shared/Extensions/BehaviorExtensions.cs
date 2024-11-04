using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Extensions
{
    public static class BehaviorExtensions
    {
        public static TDependencyObject GetDependencyObjectFromBehavior<TDependencyObject>(this DependencyObject dependencyObject) where TDependencyObject : class
        {
            if(dependencyObject is Behavior behavior) return behavior.AssociatedObject as TDependencyObject;
            return dependencyObject as TDependencyObject;
        }
    }
}
