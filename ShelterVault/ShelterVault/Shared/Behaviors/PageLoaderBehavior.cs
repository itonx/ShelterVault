using System;
using System.Collections.Generic;
using Desktiny.WinUI.Extensions;
using Desktiny.WinUI.Interfaces;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Models;
using ShelterVault.Shared.Enums;
using ShelterVault.Views;

namespace ShelterVault.Shared.Behaviors
{
    public class PageLoaderBehavior : Behavior<Frame>
    {
        public static readonly DependencyProperty SettingsPageProperty =
            DependencyProperty.Register(
                nameof(SettingsPage),
                typeof(Type),
                typeof(PageLoaderBehavior),
                new PropertyMetadata(null)
            );

        public Type SettingsPage
        {
            get { return (Type)GetValue(SettingsPageProperty); }
            set { SetValue(SettingsPageProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(LoadPagePayload),
                typeof(PageLoaderBehavior),
                new PropertyMetadata(
                    LoadPagePayload.Create(ShelterVaultPage.HOME),
                    OnSelectedItemChanged
                )
            );

        private static async void OnSelectedItemChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            Frame frame = d.GetDependencyObjectFromBehavior<Frame>();
            var loadPagePayload = (LoadPagePayload)e.NewValue;
            switch (loadPagePayload.ShelterVaultPage)
            {
                case ShelterVaultPage.HOME:
                    frame.Navigate(typeof(HomePage), loadPagePayload.Param);
                    break;
                case ShelterVaultPage.SETTINGS:
                    frame.Navigate(typeof(SettingsPage), loadPagePayload.Param);
                    break;
                case ShelterVaultPage.CREDENTIALS:
                    frame.Navigate(typeof(CredentialsPage), loadPagePayload.Param);
                    break;
                default:
                    frame.Navigate(typeof(HomePage), loadPagePayload.Param);
                    break;
            }
        }

        public LoadPagePayload SelectedItem
        {
            get { return (LoadPagePayload)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty PageTypeProperty =
            DependencyProperty.RegisterAttached(
                "PageType",
                typeof(Type),
                typeof(PageLoaderBehavior),
                new PropertyMetadata(null)
            );

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
                new PropertyMetadata(false)
            );

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
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        public static NavigationViewItem RecursiveLookup(
            IList<object> menuItems,
            object tag,
            object settingsItem
        )
        {
            if (
                tag.ToString()
                    .Equals(
                        Enums.ShelterVaultPage.SETTINGS.ToString(),
                        StringComparison.CurrentCultureIgnoreCase
                    )
            )
            {
                return (NavigationViewItem)settingsItem;
            }

            foreach (var item in menuItems)
            {
                NavigationViewItem navItem = item as NavigationViewItem;
                bool found = AreTagsEqual(navItem, tag);
                if (found)
                    return navItem;
                if (navItem.MenuItems.Count > 0)
                    return RecursiveLookup(navItem.MenuItems, tag, settingsItem);
            }

            return null;
        }

        private static bool AreTagsEqual(NavigationViewItem item, object tag)
        {
            if (item.Tag is string && tag is string && item.Tag.Equals(tag.ToString()))
            {
                return true;
            }
            if (
                item.Tag is CredentialsViewItem itemCredential
                && tag is CredentialsViewItem tagCredential
                && tagCredential.UUID.Equals(itemCredential.UUID)
            )
            {
                return true;
            }

            return false;
        }
    }
}
