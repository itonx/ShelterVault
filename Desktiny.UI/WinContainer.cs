using Desktiny.UI.Behaviors;
using Desktiny.UI.Models;
using Desktiny.UI.Tools;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System.Linq;
using Windows.UI;

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

        public static readonly DependencyProperty AppThemeProperty = DependencyProperty.Register(
            nameof(AppTheme),
            typeof(AppThemeModel),
            typeof(WinContainer),
            new PropertyMetadata(null, OnAppThemeChanged));

        private static void OnAppThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WinContainer winContainer = d as WinContainer;
            winContainer.SetApptheme((AppThemeModel)e.NewValue);
        }

        public AppThemeModel AppTheme
        {
            get { return (AppThemeModel)GetValue(AppThemeProperty); }
            set { SetValue(AppThemeProperty, value); }
        }

        private bool? _removeLastThemeResource = null;

        public static readonly DependencyProperty TitleBarMarginProperty = DependencyProperty.Register(
            "TitleBarMargin",
            typeof(Thickness),
            typeof(WinContainer),
            new PropertyMetadata(new Thickness(0, 0, 0, 0)));

        public Thickness TitleBarMargin
        {
            get { return (Thickness)GetValue(TitleBarMarginProperty); }
            set { SetValue(TitleBarMarginProperty, value); }
        }

        public static readonly DependencyProperty IsNocturneVisibleProperty = DependencyProperty.Register(
            "IsNocturneVisible",
            typeof(bool),
            typeof(WinContainer),
            new PropertyMetadata(false));

        public bool IsNocturneVisible
        {
            get { return (bool)GetValue(IsNocturneVisibleProperty); }
            set { SetValue(IsNocturneVisibleProperty, value); }
        }

        public static readonly DependencyProperty NocturneContentProperty = DependencyProperty.Register(
            "NocturneContent",
            typeof(object),
            typeof(WinContainer),
            new PropertyMetadata(null));

        public object NocturneContent
        {
            get { return GetValue(NocturneContentProperty); }
            set { SetValue(NocturneContentProperty, value); }
        }

        public WinContainer()
        {
            this.DefaultStyleKey = typeof(WinContainer);
            this.Loaded += WinContainer_Loaded;
        }

        private void WinContainer_Loaded(object sender, RoutedEventArgs e)
        {
            SetFullHeight(this.FullHeight);
            SetApptheme(this.AppTheme);
        }

        private void SetFullHeight(bool fullHeight)
        {
            Grid clientContainer = this.GetClientContainer();
            if (clientContainer == null) return;
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

        private void SetApptheme(AppThemeModel appThemeModel)
        {
            if (appThemeModel == null) return;
            Grid container = this.GetWindowContainer();
            if (container == null) return;
            ResourceDictionary lastDictionary = Application.Current.Resources.MergedDictionaries.LastOrDefault(d => d.Source.AbsoluteUri.Contains("Theme.xaml"));
            ElementTheme switchTo = appThemeModel.AppTheme == ElementTheme.Light ? ElementTheme.Dark : ElementTheme.Light;

            if (_removeLastThemeResource != null && (bool)_removeLastThemeResource && lastDictionary != null && lastDictionary.Source != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(lastDictionary);
            }

            _removeLastThemeResource = appThemeModel.ThemeResource != null;
            if (appThemeModel.ThemeResource != null)
            {
                ResourceDictionary resourceTheme = new ResourceDictionary()
                {
                    Source = appThemeModel.ThemeResource
                };
                Application.Current.Resources.MergedDictionaries.Add(resourceTheme);
            }

            container.RequestedTheme = switchTo;
            container.RequestedTheme = appThemeModel.AppTheme;
            ApplyThemeToCaptionButtons(appThemeModel.AppTheme);
        }

        private void ApplyThemeToCaptionButtons(ElementTheme currentTheme)
        {
            AppWindow appWindow = this.AppWindow;
            if (appWindow == null) return;
            appWindow.TitleBar.ButtonHoverBackgroundColor = currentTheme == ElementTheme.Light ? Color.FromArgb(50, 0, 0, 0) : Color.FromArgb(50, 255, 255, 255);
            appWindow.TitleBar.ButtonHoverForegroundColor = currentTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            appWindow.TitleBar.ButtonForegroundColor = currentTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        }
    }
}
