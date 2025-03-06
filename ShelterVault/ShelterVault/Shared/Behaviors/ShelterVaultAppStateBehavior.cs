using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Attributes;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;

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
            Frame appContainer = d.GetDependencyObjectFromBehavior<Frame>();
            ShelterVaultAppState shelterVaultAppState = (ShelterVaultAppState)e.NewValue;
            string typeStr = shelterVaultAppState.GetAttribute<PageTypeAttribute>().PageType.ToString();
            string pageTypeStr = string.Concat("ShelterVault.Views.", typeStr.Split(".").Last().Substring(1));
            appContainer.Navigate(Type.GetType(pageTypeStr));
        }

        public ShelterVaultAppState CurrentAppState
        {
            get { return (ShelterVaultAppState)GetValue(CurrentAppStateProperty); }
            set { SetValue(CurrentAppStateProperty, value); }
        }
    }
}
