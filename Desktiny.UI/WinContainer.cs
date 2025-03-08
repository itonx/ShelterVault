using Desktiny.UI.Behaviors;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Desktiny.UI
{
    public sealed class WinContainer : ContentControl
    {
        public static readonly DependencyProperty MainWindowProperty = DependencyProperty.Register(
            "MainWindow",
            typeof(Window),
            typeof(WinContainer),
            new PropertyMetadata(null));

        public Window MainWindow
        {
            get { return (Window)GetValue(MainWindowProperty); }
            set { SetValue(MainWindowProperty, value); }
        }

        public static readonly DependencyProperty AppWindowProperty = DependencyProperty.Register(
            "AppWindow",
            typeof(AppWindow),
            typeof(WinContainer),
            new PropertyMetadata(null));

        public AppWindow AppWindow
        {
            get { return (AppWindow)GetValue(AppWindowProperty); }
            set { SetValue(AppWindowProperty, value); }
        }

        public static readonly DependencyProperty TitleBarProperty = DependencyProperty.Register(
            "TitleBar",
            typeof(object),
            typeof(WinContainer),
            new PropertyMetadata(null, OnTitleBarChanged));

        private static void OnTitleBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WinContainer winContainer = d as WinContainer;
            EnableCustomAppTitleBarBehavior autoSizeAppTitleBarCaptionsBehavior = new();
            Interaction.GetBehaviors(winContainer).Add(autoSizeAppTitleBarCaptionsBehavior);
        }

        public object TitleBar
        {
            get { return GetValue(TitleBarProperty); }
            set { SetValue(TitleBarProperty, value); }
        }

        public static readonly DependencyProperty MaximizeAtStartupProperty = DependencyProperty.Register(
            "MaximizeAtStartup",
            typeof(bool),
            typeof(WinContainer),
            new PropertyMetadata(false, OnMaximizeAtStartupChanged));

        private static void OnMaximizeAtStartupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue) return;
            WinContainer winContainer = d as WinContainer;
            MaximizeWindowAtStartupBehavior maximizeWindowAtStartupBehavior = new();
            Interaction.GetBehaviors(winContainer).Add(maximizeWindowAtStartupBehavior);
        }

        public bool MaximizeAtStartup
        {
            get { return (bool)GetValue(MaximizeAtStartupProperty); }
            set { SetValue(MaximizeAtStartupProperty, value); }
        }

        public WinContainer()
        {
            this.DefaultStyleKey = typeof(WinContainer);
        }
    }
}
