using Desktiny.UI.Attributes;
using Desktiny.UI.Extensions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;

namespace Desktiny.UI.Behaviors
{
    public class EnumNavigationPageBehavior : Behavior<Frame>
    {
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(
                nameof(Target),
                typeof(object),
                typeof(EnumNavigationPageBehavior),
                new PropertyMetadata(null, OnTargetChanged));

        private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            Frame appContainer = d.GetDependencyObjectFromBehavior<Frame>();
            Enum shelterVaultAppState = (Enum)e.NewValue;
            appContainer.Navigate(shelterVaultAppState.GetAttribute<PageTypeAttribute>().PageType);
        }

        public object Target
        {
            get { return GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }
    }
}
