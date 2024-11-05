using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Models;
using ShelterVault.Shared.Extensions;
using ShelterVault.Shared.Interfaces;
using ShelterVault.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.Shared.Behaviors
{

    public class PageLoaderBehavior : Behavior<NavigationView>
    {
        public static readonly DependencyProperty SettingsPageProperty =
            DependencyProperty.Register(
                nameof(SettingsPage),
                typeof(Type),
                typeof(PageLoaderBehavior),
                new PropertyMetadata(null));

        public Type SettingsPage
        {
            get { return (Type)GetValue(SettingsPageProperty); }
            set { SetValue(SettingsPageProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(PageLoaderBehavior),
                new PropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || e.NewValue == e.OldValue) return;
            NavigationView navigationView = d.GetDependencyObjectFromBehavior<NavigationView>();
            if (navigationView.MenuItems.Count == 0) return;

            NavigationViewItem itemFound = RecursiveLookup(navigationView.MenuItems, e.NewValue);
            if (itemFound != null && (NavigationViewItem)navigationView.SelectedItem != itemFound)
            {
                bool shouldRestore = !navigationView.IsPaneOpen;
                ExpandPaneIfNeeded(navigationView);
                SelectMenuIfCollapsed(navigationView, e.NewValue);
                navigationView.SelectedItem = itemFound;
                itemFound.IsSelected = true;
                SelectMenuIfCollapsed(navigationView, e.NewValue);
                RestorePane(navigationView, shouldRestore);
            }

        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty PageTypeProperty =
            DependencyProperty.RegisterAttached(
            "PageType",
            typeof(Type),
            typeof(PageLoaderBehavior),
            new PropertyMetadata(null));

        public static void SetPageType(NavigationViewItem obj, Type value)
        {
            obj.SetValue(PageTypeProperty, value);
        }

        public static Type GetPageType(NavigationViewItem obj)
        {
            return (Type)obj.GetValue(PageTypeProperty);
        }

        public static readonly DependencyProperty SkipSelectionProperty =
            DependencyProperty.RegisterAttached(
            "SkipSelection",
            typeof(bool),
            typeof(PageLoaderBehavior),
            new PropertyMetadata(false));

        public static void SetSkipSelection(NavigationView obj, bool value)
        {
            obj.SetValue(SkipSelectionProperty, value);
        }

        public static bool GetSkipSelection(NavigationView obj)
        {
            return (bool)obj.GetValue(SkipSelectionProperty);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private async void AssociatedObject_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem == null) return;
            bool? skip = (bool?)AssociatedObject.GetValue(PageLoaderBehavior.SkipSelectionProperty);
            if (skip == true) return;
            if (AssociatedObject.Content is not Frame pageContainer) throw new InvalidOperationException("The NavigationView must contain a Frame.");

            NavigationViewItem selectedItem = (NavigationViewItem)args.SelectedItem;
            object lastSelectedItemByTag = this.SelectedItem;

            if(pageContainer.Content is Page page && page.DataContext is IPendingChangesChallenge pendingChangesChallenge && !pendingChangesChallenge.ChallengeCompleted)
            {
                bool discardChanges = await pendingChangesChallenge.DiscardChangesAsync();
                if(!discardChanges)
                {
                    NavigationViewItem itemFound = RecursiveLookup(sender.MenuItems, lastSelectedItemByTag);
                    AssociatedObject.SetValue(PageLoaderBehavior.SkipSelectionProperty, true);
                    sender.SelectedItem = itemFound;
                    SelectMenuIfCollapsed(AssociatedObject, itemFound.Tag);
                    AssociatedObject.SetValue(PageLoaderBehavior.SkipSelectionProperty, false);
                    return;
                }
            }

            if (args.IsSettingsSelected)
            {
                pageContainer.Navigate(SettingsPage);
            }
            else
            {
                Type selectedPageType = (Type)selectedItem.GetValue(PageTypeProperty);
                object navigationParameter = selectedItem.Tag;
                this.SelectedItem = navigationParameter;
                if (selectedPageType == null) return;
                pageContainer.Navigate(selectedPageType, navigationParameter);
            }
        }

        public static NavigationViewItem RecursiveLookup(IList<object> menuItems, object tag)
        {
            foreach (var item in menuItems)
            {
                NavigationViewItem navItem = item as NavigationViewItem;
                bool found = AreTagEqual(navItem, tag);
                if (found) return navItem;
                if (navItem.MenuItems.Count > 0) return RecursiveLookup(navItem.MenuItems, tag);
            }

            return null;
        }

        private static bool AreTagEqual(NavigationViewItem item, object tag)
        {
            if (item.Tag is string && tag is string && item.Tag.Equals(tag.ToString()))
            {
                return true;
            }
            if (item.Tag is Credential itemCredential && tag is Credential tagCredential && tagCredential.UUID.Equals(itemCredential.UUID))
            {
                return true;
            }

            return false;
        }

        private static void SelectMenuIfCollapsed(NavigationView navigationView, object tag)
        {
            if (tag is Credential && navigationView.MenuItems.Count > 2)
            {
                NavigationViewItem menuItem = navigationView.MenuItems[2] as NavigationViewItem;
                if (!menuItem.IsExpanded)
                {
                    menuItem.IsExpanded = true;
                    menuItem.UpdateLayout();
                    menuItem.IsExpanded = false; 
                    menuItem.UpdateLayout();
                }
            }
        }

        private static void ExpandPaneIfNeeded(NavigationView navigationView)
        {
            if (!navigationView.IsPaneOpen)
            {
                navigationView.IsPaneOpen = true;
                navigationView.UpdateLayout();
            }
        }

        private static void RestorePane(NavigationView navigationView, bool shouldRestore)
        {
            if (shouldRestore)
            {
                navigationView.IsPaneOpen = false;
                navigationView.UpdateLayout();
            }
        }
    }
}
