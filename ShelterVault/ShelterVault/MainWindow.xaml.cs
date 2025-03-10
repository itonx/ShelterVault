using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using ShelterVault.ViewModels;

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
            this.WindowContainer.DataContext = App.Current.Services.GetService<MainWindowViewModel>();
        }
    }
}
