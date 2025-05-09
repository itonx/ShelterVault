using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Desktiny.UI.Behaviors
{
    public class EnableCustomAppTitleBarBehavior : Behavior<WinContainer>
    {

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.TitleBar.Loaded += AppTitleBar_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            if (AssociatedObject.TitleBar == null) return;
            AssociatedObject.TitleBar.Loaded -= AppTitleBar_Loaded;
        }

        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            Window mainWindow = AssociatedObject.MainWindow;

            AssociatedObject.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            AssociatedObject.AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
            mainWindow.SetTitleBar(AssociatedObject.TitleBar);
        }
    }
}
