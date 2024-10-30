using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using ShelterVault.ViewModels;
using ShelterVault.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using WinRT.Interop;
 
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
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
            this.WindowContainer.DataContext = App.Current.Services.GetService<MainWindowViewModel>();
            m_AppWindow = GetAppWindowForCurrentWindow();
            m_AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            
            AppTitleBar.Height = (double)m_AppWindow.TitleBar.Height;
            AppTitleBar.Loaded += AppTitleBar_Loaded;
            (this.Content as Grid).ActualThemeChanged += MainWindow_ActualThemeChanged;

            SetCaptionButtons((this.Content as FrameworkElement).ActualTheme);
        }

        private void MainWindow_ActualThemeChanged(FrameworkElement sender, object args)
        {
            var actual = sender.ActualTheme;
            var requestedx = sender.RequestedTheme;
            SetCaptionButtons(actual);
        }

        private void SetCaptionButtons(ElementTheme currentTheme)
        {
            m_AppWindow.TitleBar.ButtonHoverBackgroundColor = currentTheme == ElementTheme.Light ? Color.FromArgb(50, 0, 0, 0) : Color.FromArgb(50, 255, 255, 255);
            m_AppWindow.TitleBar.ButtonForegroundColor = currentTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            m_AppWindow.TitleBar.ButtonHoverForegroundColor = currentTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        }

        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;
            RightPaddingColumn.Width = new GridLength(m_AppWindow.TitleBar.RightInset / scaleAdjustment);
            LeftPaddingColumn.Width = new GridLength(m_AppWindow.TitleBar.LeftInset / scaleAdjustment);
        }

        private AppWindow m_AppWindow;

        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }
    }
}
