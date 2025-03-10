using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Desktiny.UI.Views
{
    public sealed partial class SimpleDialogView : UserControl
    {
        public SimpleDialogView()
        {
            this.InitializeComponent();
        }

        public SimpleDialogView(string message) : this()
        {
            this.Message.Text = message;
        }
    }
}
