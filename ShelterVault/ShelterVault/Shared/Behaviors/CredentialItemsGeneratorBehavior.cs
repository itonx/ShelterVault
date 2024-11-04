using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Models;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using ShelterVault.Views;
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
            DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList<Credential>),
            typeof(CredentialItemsGeneratorBehavior),
            new PropertyMetadata(null, OnItemsSourceChanged));

        public IList<Credential> ItemsSource
        {
            get { return (IList<Credential>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            NavigationViewItem item = d.GetDependencyObjectFromBehavior<NavigationViewItem>();
            item.MenuItems.Clear();
            foreach (Credential credential in (IList<Credential>)e.NewValue)
            {
                NavigationViewItem navigationViewItem = new() { Content = credential.Title, Icon = new FontIcon() { Glyph= "\uE72E" }, Tag = credential };
                navigationViewItem.SetValue(ToolTipService.ToolTipProperty, credential.Title);
                navigationViewItem.SetValue(PageLoaderBehavior.PageTypeProperty, typeof(CredentialsPage));
                item.MenuItems.Add(navigationViewItem);
            }
            if(item.MenuItems.Count == 0) item.Visibility = Visibility.Collapsed;
            else if(item.Visibility == Visibility.Collapsed) item.Visibility = Visibility.Visible;
        }
    }
}
