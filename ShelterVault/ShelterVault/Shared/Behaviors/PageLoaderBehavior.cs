using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
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

        private void AssociatedObject_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (AssociatedObject.Content is not Frame pageContainer) throw new InvalidOperationException("The NavigationView must contain a Frame.");

            if (args.IsSettingsSelected)
            {
                pageContainer.Navigate(SettingsPage);
            }
            else
            {
                if (args.SelectedItem == null) return;
                NavigationViewItem selectedItem = (NavigationViewItem)args.SelectedItem;
                object navigationParameter = selectedItem.Tag as object;
                Type selectedPageType = (Type)selectedItem.GetValue(PageTypeProperty);
                if (selectedPageType == null) return;
                pageContainer.Navigate(selectedPageType, navigationParameter);
            }
        }
    }
}
