using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShelterVault.Views
{
    public sealed partial class ShelterVaultMessageView : UserControl
    {
        public ShelterVaultMessageView()
        {
            this.InitializeComponent();
        }

        public ShelterVaultMessageView(string message) : this()
        {
            this.Message.Text = message;
        }
    }
}
