using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Messages;

namespace ShelterVault.Shared.Behaviors
{
    class PaneToggleRequestedBehavior : Behavior<TitleBar>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PaneToggleRequested += AssociatedObject_PaneToggleRequested;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PaneToggleRequested -= AssociatedObject_PaneToggleRequested;
        }

        private void AssociatedObject_PaneToggleRequested(TitleBar sender, object args)
        {
            WeakReferenceMessenger.Default.Send(new IsPaneOpenMessage(true));
        }
    }
}
