using Microsoft.UI.Xaml;
using ShelterVault.Shared.Behaviors;

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
