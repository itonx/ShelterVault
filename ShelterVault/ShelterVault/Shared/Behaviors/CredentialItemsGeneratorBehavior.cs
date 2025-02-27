using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Models;
using ShelterVault.Shared.Extensions;
using ShelterVault.Views;
using System.Collections.Generic;

namespace ShelterVault.Shared.Behaviors
{
    public class CredentialItemsGeneratorBehavior : Behavior<NavigationViewItem>
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList<CredentialsViewItem>),
            typeof(CredentialItemsGeneratorBehavior),
            new PropertyMetadata(null, OnItemsSourceChanged));

        public IList<CredentialsViewItem> ItemsSource
        {
            get { return (IList<CredentialsViewItem>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            NavigationViewItem item = d.GetDependencyObjectFromBehavior<NavigationViewItem>();
            item.MenuItems.Clear();
            foreach (CredentialsViewItem credentialsViewItem in (IList<CredentialsViewItem>)e.NewValue)
            {
                NavigationViewItem navigationViewItem = new() { Content = credentialsViewItem.Title, Icon = new FontIcon() { Glyph = "\uE72E" }, Tag = credentialsViewItem };
                navigationViewItem.SetValue(ToolTipService.ToolTipProperty, credentialsViewItem.Title);
                navigationViewItem.SetValue(PageLoaderBehavior.PageTypeProperty, typeof(CredentialsPage));
                item.MenuItems.Add(navigationViewItem);
            }
            if (item.MenuItems.Count == 0) item.Visibility = Visibility.Collapsed;
            else if (item.Visibility == Visibility.Collapsed) item.Visibility = Visibility.Visible;
            item.IsChildSelected = false;
            item.UpdateLayout();
        }
    }
}
