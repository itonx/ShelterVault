using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
 
namespace ShelterVault.Shared.Helpers
{
    public static class MultiPasswordBoxValuesHelper
    {
        public static readonly DependencyProperty SecurePasswordsProperty =
            DependencyProperty.RegisterAttached("SecurePasswords", typeof(Dictionary<string, StringBuilder>), typeof(MultiPasswordBoxValuesHelper), new PropertyMetadata(null));

        public static Dictionary<string, StringBuilder> GetSecurePasswords(DependencyObject obj) => (Dictionary<string, StringBuilder>)obj.GetValue(SecurePasswordsProperty);

        public static void SetSecurePasswords(DependencyObject obj, Dictionary<string, StringBuilder> value) => obj.SetValue(SecurePasswordsProperty, value);

        public static readonly DependencyProperty PasswordChangedToCommandProperty =
            DependencyProperty.RegisterAttached("PasswordChangedToCommand", typeof(ICommand), typeof(MultiPasswordBoxValuesHelper), new PropertyMetadata(null, OnPasswordChangedToCommandChanged));

        private static void OnPasswordChangedToCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is PasswordBox passwordBox)
            {
                passwordBox.Loaded += PasswordBox_Loaded;
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged_ExecuteCommand;
            }
        }

        private static void PasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as PasswordBox).Loaded -= PasswordBox_Loaded;
            PasswordBox_PasswordChanged_ExecuteCommand(sender, e);
        }

        private static void PasswordBox_PasswordChanged_ExecuteCommand(object sender, RoutedEventArgs e)
        {
            if(sender is PasswordBox passwordBox)
            {
                ICommand passwordChangedToCommand = GetPasswordChangedToCommand(passwordBox);
                if (passwordChangedToCommand != null) passwordChangedToCommand.Execute(passwordBox.Password);
            }
        }

        public static ICommand GetPasswordChangedToCommand(DependencyObject obj) => (ICommand)obj.GetValue(PasswordChangedToCommandProperty);

        public static void SetPasswordChangedToCommand(DependencyObject obj, ICommand value) => obj.SetValue(PasswordChangedToCommandProperty, value);


        public static readonly DependencyProperty PasswordBoxConfirmationProperty =
            DependencyProperty.RegisterAttached("PasswordBoxConfirmation", typeof(PasswordBox), typeof(MultiPasswordBoxValuesHelper), new PropertyMetadata(null, OnPasswordBoxConfirmationChanged));

        public static PasswordBox GetPasswordBoxConfirmation(DependencyObject obj) => (PasswordBox)obj.GetValue(PasswordBoxConfirmationProperty);

        public static void SetPasswordBoxConfirmation(DependencyObject obj, PasswordBox value) => obj.SetValue(PasswordBoxConfirmationProperty, value);

        private static void OnPasswordBoxConfirmationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.OldValue == null)
            {
                if (d is PasswordBox passwordBox && e.NewValue is PasswordBox passwordBoxConfirmation)
                {
                    if (string.IsNullOrWhiteSpace(passwordBox.Name) || string.IsNullOrWhiteSpace(passwordBoxConfirmation.Name)) throw new Exception("Names are not valid!");
                    Dictionary<string, StringBuilder> passwords = new Dictionary<string, StringBuilder>()
                    {
                        { passwordBox.Name, new StringBuilder() },
                        { passwordBoxConfirmation.Name, new StringBuilder() }
                    };
                    SetSecurePasswords(passwordBox, passwords);
                    SetSecurePasswords(passwordBoxConfirmation, passwords);
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                    passwordBoxConfirmation.PasswordChanged += PasswordBox_PasswordChanged;
                }
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                Dictionary<string, StringBuilder> passwords = GetSecurePasswords(passwordBox);
                if(passwords != null)
                {
                    passwords[passwordBox.Name].Clear();
                    passwords[passwordBox.Name].Append(passwordBox.Password);
                    SetSecurePasswords(passwordBox, passwords);
                }

                PasswordBox_PasswordChanged_ExecuteCommand(sender, e);
            }
        }
    }
}
