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
    public class AppStateLoaderBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty CurrentAppStateProperty =
            DependencyProperty.Register(
                nameof(CurrentAppState),
                typeof(ShelterVaultAppState),
                typeof(AppStateLoaderBehavior),
                new PropertyMetadata(ShelterVaultAppState.CreateMasterKey, OnCurrentAppStateChanged));

        private static void OnCurrentAppStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AppStateLoaderBehavior control = d as AppStateLoaderBehavior;
            Grid gridContainer = control.AssociatedObject as Grid;
            Frame appContainer = gridContainer.Children.FirstOrDefault(e => e is Frame) as Frame;
            if (appContainer == null) throw new InvalidOperationException("Frame not found.");
            appContainer.Navigate(control.CurrentAppState.GetAttribute<PageTypeAttribute>().PageType);
        }

        public ShelterVaultAppState CurrentAppState
        {
            get { return (ShelterVaultAppState)GetValue(CurrentAppStateProperty); }
            set { SetValue(CurrentAppStateProperty, value); }
        }
    }
}
