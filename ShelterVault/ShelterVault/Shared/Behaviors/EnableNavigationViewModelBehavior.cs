using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Models;
using ShelterVault.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Behaviors
{
    public class EnableNavigationViewModelBehavior : Behavior<Frame>
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

            if(e.Parameter != null && pageRequested.DataContext is INavigation navigate)
            {
                if (e.Parameter is not string) navigate.OnNavigateTo(e.Parameter);
            }
        }
    }
}
