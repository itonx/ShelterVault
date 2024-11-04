using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.Shared.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShelterVault.Shared.Extensions
{
    public static class PasswordBoxExtensions
    {
        public static void ExecuteAttachedCommand(this ExecuteCommandOnPasswordChangedBehavior passwordBoxBehavior, DependencyProperty dependencyProperty)
        {
            passwordBoxBehavior?.Command?.Execute(passwordBoxBehavior.AssociatedObject?.Password ?? null);
        }
    }
}
