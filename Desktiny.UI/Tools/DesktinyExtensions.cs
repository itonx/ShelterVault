using Microsoft.UI.Xaml.Controls;

namespace Desktiny.UI.Tools
{
    public static class DesktinyExtensions
    {
        public static Grid GetAppTitleBar(this WinContainer winContainer)
        {
            return WinUI3Helper.FindChildElementByName(winContainer, "AppTitleBar") as Grid;
        }
    }
}
