using Microsoft.UI.Xaml.Controls;
using ShelterVault.Core.Shared.Interfaces;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShelterVault.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page, IHomePage
    {
        public HomePage()
        {
            this.InitializeComponent();
        }
    }
}
