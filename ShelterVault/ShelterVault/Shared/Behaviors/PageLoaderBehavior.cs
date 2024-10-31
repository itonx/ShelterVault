using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
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

        public static readonly DependencyProperty SkipSelectionLoginProperty =
            DependencyProperty.RegisterAttached(
            "SkipSelectionLogin",
            typeof(bool),
            typeof(PageLoaderBehavior),
            new PropertyMetadata(false));

        public static void SetSkipSelectionLogin(NavigationView obj, bool value)
        {
            obj.SetValue(SkipSelectionLoginProperty, value);
        }

        public static bool GetSkipSelectionLogin(NavigationView obj)
        {
            return (bool)obj.GetValue(SkipSelectionLoginProperty);
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
            bool? skip = (bool?)sender.GetValue(SkipSelectionLoginProperty);
            if (skip == true) return;
            if (AssociatedObject.Content is not Frame pageContainer) throw new InvalidOperationException("The NavigationView must contain a Frame.");

            NavigationViewItem selectedItem = (NavigationViewItem)args.SelectedItem;
            object lastSelectedItemByTag = AssociatedObject.GetValue(SelectItemByTagBehavior.SelectedItemProperty);

            if(pageContainer.Content is Page page && page.DataContext is IPendingChangesChallenge pendingChangesChallenge && !pendingChangesChallenge.ChallengeCompleted)
            {
                bool discardChanges = await pendingChangesChallenge.DiscardChangesAsync();
                if(!discardChanges)
                {
                    NavigationViewItem itemFound = SelectItemByTagBehavior.RecursiveLookup(sender.MenuItems, lastSelectedItemByTag);
                    sender.SetValue(SkipSelectionLoginProperty, true);
                    sender.SelectedItem = itemFound;
                    sender.SetValue(SkipSelectionLoginProperty, false);
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
                if (selectedPageType == null) return;
                pageContainer.Navigate(selectedPageType, navigationParameter);
            }
        }
    }
}
