using Desktiny.UI.Behaviors;
using Desktiny.UI.Tools;
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

        public static readonly DependencyProperty FullHeightProperty = DependencyProperty.Register(
            "FullHeight",
            typeof(bool),
            typeof(WinContainer),
            new PropertyMetadata(true, OnFullHeightChanged));

        private static void OnFullHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WinContainer winContainer = d as WinContainer;
            winContainer.SetFullHeight((bool)e.NewValue);
        }

        public bool FullHeight
        {
            get { return (bool)GetValue(FullHeightProperty); }
            set { SetValue(FullHeightProperty, value); }
        }

        public WinContainer()
        {
            this.DefaultStyleKey = typeof(WinContainer);
            this.Loaded += WinContainer_Loaded;
        }

        private void WinContainer_Loaded(object sender, RoutedEventArgs e)
        {
            SetFullHeight(this.FullHeight);
        }

        private void SetFullHeight(bool fullHeight)
        {
            Grid clientContainer = this.GetClientContainer();
            if (fullHeight)
            {
                clientContainer.SetValue(Grid.RowProperty, 0);
                clientContainer.SetValue(Grid.RowSpanProperty, 2);
            }
            else
            {
                clientContainer.SetValue(Grid.RowProperty, 1);
                clientContainer.SetValue(Grid.RowSpanProperty, 1);
            }
        }
    }
}
