using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Behaviors
{
    public class AddMarginOnNavigationViewLoadedBehavior : Behavior<Grid>
    {
        public static readonly DependencyProperty ShelterVaultCurrentAppStateProperty =
            DependencyProperty.Register(
                nameof(ShelterVaultCurrentAppState),
                typeof(ShelterVaultAppState),
                typeof(AddMarginOnNavigationViewLoadedBehavior),
                new PropertyMetadata(null, OnShelterVaultCurrentAppStateChanged));

        private static void OnShelterVaultCurrentAppStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            Grid grid = d.GetDependencyObjectFromBehavior<Grid>();
            ShelterVaultAppState currentState = (ShelterVaultAppState)e.NewValue;
            if (currentState == ShelterVaultAppState.NavigationView) grid.Margin = new Thickness(50, 0, 0, 0);
            else grid.Margin = new Thickness(0);
        }

        public ShelterVaultAppState ShelterVaultCurrentAppState
        {
            get { return (ShelterVaultAppState)GetValue(ShelterVaultCurrentAppStateProperty); }
            set { SetValue(ShelterVaultCurrentAppStateProperty, value); }
        }
    }
}
