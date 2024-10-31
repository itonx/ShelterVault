using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using ShelterVault.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xaml.Interactivity;

namespace ShelterVault.Shared.Behaviors
{
    public class SelectItemByTagBehavior : Behavior<NavigationView>
    {
        public static readonly DependencyProperty SelectedItemProperty =
         DependencyProperty.RegisterAttached(
         "SelectedItem",
         typeof(object),
         typeof(SelectItemByTagBehavior),
         new PropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            NavigationView navigationView = d as NavigationView;
            if (navigationView.MenuItems.Count == 0) return;

            NavigationViewItem itemFound = RecursiveLookup(navigationView.MenuItems, e.NewValue);
            if (itemFound != null) navigationView.SelectedItem = itemFound;
        }

        public static NavigationViewItem RecursiveLookup(IList<object> menuItems, object tag)
        {
            foreach (var item in menuItems)
            {
                NavigationViewItem navItem = item as NavigationViewItem;
                bool found = AreTagEqual(navItem, tag);
                if (found) return navItem;

                if (!found && navItem.MenuItems.Count > 0)
                {
                    return RecursiveLookup(navItem.MenuItems, tag);
                }

            }

            return null;
        }

        private static bool AreTagEqual(NavigationViewItem item, object tag)
        {
            if (item.Tag is string && tag is string && item.Tag.Equals(tag.ToString()))
            {
                return true;
            }
            if(item.Tag is Credential itemCredential && tag is Credential tagCredential && tagCredential.UUID.Equals(itemCredential.UUID))
            {
                return true;
            }

            return false;
        }

        public static void SetSelectedItem(NavigationView obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        public static object GetSelectedItem(NavigationView obj)
        {
            return obj.GetValue(SelectedItemProperty);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            sender.SetValue(SelectedItemProperty, (sender.SelectedItem as NavigationViewItem).Tag);
        }
    }
}
