using Desktiny.UI.Tools;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Desktiny.UI.Services
{
    public interface IAppWindow
    {
        Window MainWindow { get; }
    }

    public interface IWindowService
    {
        public Window GetMainWindow();
        public AppWindow GetAppWindowWindow();
    }

    public class WindowService : IWindowService
    {
        public AppWindow GetAppWindowWindow()
        {
            return WindowHelper.CurrentAppWindow;
        }

        public Window GetMainWindow()
        {
            return WindowHelper.CurrentMainWindow;
        }
    }
}
