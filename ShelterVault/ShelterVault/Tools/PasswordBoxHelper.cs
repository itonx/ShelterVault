using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.Tools
{
    public static class PasswordBoxHelper
    {

        public static readonly DependencyProperty SecurePasswordProperty =
            DependencyProperty.RegisterAttached("SecurePassword", typeof(string), typeof(PasswordBoxHelper), new PropertyMetadata(null, OnSecurePasswordChanged));

        public static string GetSecurePassword(DependencyObject obj) => (string)obj.GetValue(SecurePasswordProperty);

        public static void SetSecurePassword(DependencyObject obj, string value) => obj.SetValue(SecurePasswordProperty, value);

        private static void OnSecurePasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox && e.NewValue != null && (string)e.NewValue != passwordBox.Password)
            {
                passwordBox.Password = (string)e.NewValue;
            }
        }

        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached("IsMonitoringProperty", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false, OnIsMonitoringChanged));

        public static bool GetIsMonitoring(DependencyObject obj) => (bool)obj.GetValue(IsMonitoringProperty);

        public static void SetIsMonitoring(DependencyObject obj, bool value) => obj.SetValue(IsMonitoringProperty, value);

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox && (bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
            else if(d is PasswordBox passwordBoxTmp)
            {
                passwordBoxTmp.PasswordChanged -= PasswordBox_PasswordChanged;
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetSecurePassword(passwordBox, passwordBox.Password);
            }
        }
    }
}
