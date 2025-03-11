using Desktiny.UI.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace Desktiny.UI.Behaviors
{
    public class ExecuteOnINavigationImplementedBehavior : Behavior<Frame>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Navigated += AssociatedObject_Navigated;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Navigated -= AssociatedObject_Navigated;
        }

        private void AssociatedObject_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            Frame pageContainer = sender as Frame;
            Page pageRequested = pageContainer.Content as Page;

            if (e.Parameter != null && pageRequested.DataContext is INavigation navigate && e.Parameter is not string)
            {
                navigate.OnNavigated(e.Parameter);
            }
        }
    }
}
