using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Attributes;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using ShelterVault.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.Shared.Behaviors
{
    public class ShelterVaultAppStateBehavior : Behavior<Frame>
    {
        public static readonly DependencyProperty CurrentAppStateProperty =
            DependencyProperty.Register(
                nameof(CurrentAppState),
                typeof(ShelterVaultAppState),
                typeof(ShelterVaultAppStateBehavior),
                new PropertyMetadata(ShelterVaultAppState.CreateMasterKey, OnCurrentAppStateChanged));

        private static void OnCurrentAppStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ShelterVaultAppStateBehavior behavior = d as ShelterVaultAppStateBehavior;
            Frame appContainer = behavior.AssociatedObject;
            appContainer.Navigate(behavior.CurrentAppState.GetAttribute<PageTypeAttribute>().PageType);
        }

        public ShelterVaultAppState CurrentAppState
        {
            get { return (ShelterVaultAppState)GetValue(CurrentAppStateProperty); }
            set { SetValue(CurrentAppStateProperty, value); }
        }
    }
}
