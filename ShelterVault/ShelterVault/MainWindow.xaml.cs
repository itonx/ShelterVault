using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using ShelterVault.Views;
using System;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShelterVault
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
        }

        private void NavigationView_SelectionChanged8(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            /* NOTE: for this function to work, every NavigationView must follow the same naming convention: nvSample# (i.e. nvSample3),
            and every corresponding content frame must follow the same naming convention: contentFrame# (i.e. contentFrame3) */

            // Get the sample number
            string sampleNum = (sender.Name).Substring(8);
            Debug.Print("num: " + sampleNum + "\n");

            if (args.IsSettingsSelected)
            {
                this.contentFrame8.Navigate(typeof(SettingsPage));
            }
            else
            {
                if (args.SelectedItem == null) return;
                var selectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
                string selectedItemTag = ((string)selectedItem.Tag);
                if (string.IsNullOrWhiteSpace(selectedItemTag)) return;
                sender.Header = "Sample Page " + selectedItemTag.Substring(selectedItemTag.Length - 1);
                string pageName = "ShelterVault.Views." + selectedItemTag;
                Type pageType = Type.GetType(pageName);
                if (pageType == null) return;
                this.contentFrame8.Navigate(pageType);
            }
        }
    }
}
