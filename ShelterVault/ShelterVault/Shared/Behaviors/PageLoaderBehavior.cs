using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Models;
using ShelterVault.Shared.Extensions;
using ShelterVault.Shared.Interfaces;
using System;
using System.Collections.Generic;

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

        private static async void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (d as PageLoaderBehavior);
            if (e.NewValue == null || e.NewValue == e.OldValue || (bool)behavior.GetValue(SkipSelectionProperty)) return;
            NavigationView navigationView = d.GetDependencyObjectFromBehavior<NavigationView>();
            if (navigationView.Content is not Frame pageContainer) throw new InvalidOperationException("The NavigationView must contain a Frame.");
            if (e.NewValue.ToString().Equals(Enums.ShelterVaultPage.SETTINGS.ToString(), StringComparison.CurrentCultureIgnoreCase) && pageContainer.Content is Page page && page.DataContext is IPendingChangesChallenge pendingChangesChallenge && !pendingChangesChallenge.ChallengeCompleted)
            {
                bool discardChanges = await pendingChangesChallenge.DiscardChangesAsync(completeChallenge: true);
                if (!discardChanges)
                {
                    behavior.SetValue(SkipSelectionProperty, true);
                    behavior.SelectedItem = e.OldValue;
                    behavior.SetValue(SkipSelectionProperty, false);
                    return;
                }
            }

            if (navigationView.MenuItems.Count == 0) return;

            NavigationViewItem itemFound = RecursiveLookup(navigationView.MenuItems, e.NewValue, navigationView.SettingsItem);
            if (itemFound != null && (NavigationViewItem)navigationView.SelectedItem != itemFound)
            {
                bool shouldRestore = !navigationView.IsPaneOpen;
                if (!e.NewValue.ToString().Equals(Enums.ShelterVaultPage.HOME.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    ExpandPaneIfNeeded(navigationView);
                    SelectMenuIfCollapsed(navigationView, e.NewValue);
                }
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

            if (pageContainer.Content is Page page && page.DataContext is IPendingChangesChallenge pendingChangesChallenge && !pendingChangesChallenge.ChallengeCompleted && ((selectedItem.Tag is CredentialsViewItem credentialTmp && !credentialTmp.SkipPageLoader) || (selectedItem.Tag is string)))
            {
                bool discardChanges = await pendingChangesChallenge.DiscardChangesAsync();
                if (!discardChanges)
                {
                    NavigationViewItem itemFound = RecursiveLookup(sender.MenuItems, lastSelectedItemByTag, sender.SettingsItem);
                    AssociatedObject.SetValue(PageLoaderBehavior.SkipSelectionProperty, true);
                    sender.SelectedItem = itemFound;
                    SelectMenuIfCollapsed(AssociatedObject, itemFound.Tag);
                    AssociatedObject.SetValue(PageLoaderBehavior.SkipSelectionProperty, false);
                    return;
                }
            }

            if (args.IsSettingsSelected)
            {
                this.SelectedItem = Enums.ShelterVaultPage.SETTINGS.ToString();
                pageContainer.Navigate(SettingsPage);
            }
            else
            {
                Type selectedPageType = (Type)selectedItem.GetValue(PageTypeProperty);
                object navigationParameter = selectedItem.Tag;
                this.SelectedItem = navigationParameter;
                if (selectedPageType == null) return;
                if (navigationParameter is CredentialsViewItem credential && credential.SkipPageLoader) return;
                pageContainer.Navigate(selectedPageType, navigationParameter);
            }
        }

        public static NavigationViewItem RecursiveLookup(IList<object> menuItems, object tag, object settingsItem)
        {
            if (tag.ToString().Equals(Enums.ShelterVaultPage.SETTINGS.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                return (NavigationViewItem)settingsItem;
            }

            foreach (var item in menuItems)
            {
                NavigationViewItem navItem = item as NavigationViewItem;
                bool found = AreTagsEqual(navItem, tag);
                if (found) return navItem;
                if (navItem.MenuItems.Count > 0) return RecursiveLookup(navItem.MenuItems, tag, settingsItem);
            }

            return null;
        }

        private static bool AreTagsEqual(NavigationViewItem item, object tag)
        {
            if (item.Tag is string && tag is string && item.Tag.Equals(tag.ToString()))
            {
                return true;
            }
            if (item.Tag is CredentialsViewItem itemCredential && tag is CredentialsViewItem tagCredential && tagCredential.UUID.Equals(itemCredential.UUID))
            {
                return true;
            }

            return false;
        }

        private static void SelectMenuIfCollapsed(NavigationView navigationView, object tag)
        {
            if (tag is CredentialsViewItem && navigationView.MenuItems.Count > 2)
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
