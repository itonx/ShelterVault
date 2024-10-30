using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace ShelterVault.Shared.Helpers
{
    public static class RequestFocusHelper
    {
        public static readonly DependencyProperty FocusProperty =
            DependencyProperty.RegisterAttached("Focus", typeof(bool), typeof(RequestFocusHelper), new PropertyMetadata(false, OnFocusChanged));

        public static bool GetFocus(DependencyObject obj) => (bool)obj.GetValue(FocusProperty);

        public static void SetFocus(DependencyObject obj, bool value) => obj.SetValue(FocusProperty, value);

        private static void OnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is FrameworkElement control && e.NewValue is bool curVal && e.OldValue is bool oldVal && curVal && !oldVal)
            {
                control.Loaded += Control_Loaded;
            }
        }

        private static void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if(sender is FrameworkElement control)
            {
                control.Loaded -= Control_Loaded;
                control.Focus(FocusState.Programmatic);
            }
        }

        public static readonly DependencyProperty RequestFocusProperty =
            DependencyProperty.RegisterAttached("RequestFocus", typeof(bool), typeof(RequestFocusHelper), new PropertyMetadata(false, OnRequestFocusChanged));

        public static bool GetRequestFocus(DependencyObject obj) => (bool)obj.GetValue(RequestFocusProperty);

        public static void SetRequestFocus(DependencyObject obj, bool value) => obj.SetValue(RequestFocusProperty, value);

        private static void OnRequestFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement control && e.NewValue is bool curVal && e.OldValue is bool oldVal && curVal && !oldVal)
            {
                control.Focus(FocusState.Programmatic);
                if(control is TextBox textBox) textBox.SelectionStart = textBox.Text.Length;
            }
            SetRequestFocus(d, false);
        }
    }
}
