using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShelterVault.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CredentialsPage : Page
    {
        public CredentialsPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Current.Services.GetService<CredentialsViewModel>();
        }

        private void MenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var vm = this.DataContext as CredentialsViewModel;
            if (vm != null)
            {
                vm.DeleteUrlCommand.Execute((sender as MenuFlyoutItem).Tag);
            }
        }

        private void SelectedPassword_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            PasswordConfirmationView.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }

        private void SelectedPasswordConfirmation_LostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            PasswordConfirmationView.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }
}
