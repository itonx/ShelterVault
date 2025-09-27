using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ShelterVault.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShelterVault.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateMasterKeyPage : Page
    {
        public CreateMasterKeyPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Current.Services.GetService<CreateMasterKeyViewModel>();
        }

        private void password_GotFocus(object sender, RoutedEventArgs e)
        {
            passwordTeachingTip.IsOpen = true;
        }

        private void password_LostFocus(object sender, RoutedEventArgs e)
        {
            passwordTeachingTip.IsOpen = false;
        }
    }
}
