using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Shared.Behaviors
{
    public class CredentialItemsGeneratorBehavior : Behavior<NavigationViewItem>
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.RegisterAttached(
            "ItemsSource",
            typeof(IList<Credential>),
            typeof(CredentialItemsGeneratorBehavior),
            new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            NavigationViewItem item = d as NavigationViewItem;
            item.MenuItems.Clear();
            foreach (Credential credential in (IList<Credential>)e.NewValue)
            {
                NavigationViewItem navigationViewItem = new() { Content = credential.Title, Icon = new FontIcon() { Glyph= "\uE72E" }, Tag = credential };
                navigationViewItem.SetValue(ToolTipService.ToolTipProperty, credential.Title);
                item.MenuItems.Add(navigationViewItem);
            }
            if(item.MenuItems.Count == 0) item.Visibility = Visibility.Collapsed;
        }

        public static void SetItemsSource(NavigationViewItem obj, IList<Credential> value)
        {
            obj.SetValue(ItemsSourceProperty, value);
        }

        public static IList<Credential> GetItemsSource(NavigationViewItem obj)
        {
            return (IList<Credential>)obj.GetValue(ItemsSourceProperty);
        }
    }
}
