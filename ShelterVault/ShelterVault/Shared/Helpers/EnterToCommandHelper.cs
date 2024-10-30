using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
 
namespace ShelterVault.Shared.Helpers
{
    public static class EnterToCommandHelper
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(EnterToCommandHelper), new PropertyMetadata(null, OnCommandChanged));

        public static ICommand GetCommand(DependencyObject obj) => (ICommand)obj.GetValue(CommandProperty);

        public static void SetCommand(DependencyObject obj, ICommand value) => obj.SetValue(CommandProperty, value);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.OldValue == null)
            {
                FrameworkElement control = d as FrameworkElement;
                d.SetValue(CommandProperty, e.NewValue);
                control.KeyDown += Control_KeyDown;
            }
        }

        private static void Control_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && sender is FrameworkElement control)
            {
                ICommand command = GetCommand(control);
                object commandParameter = GetCommandParameter(control);
                bool execute = command.CanExecute(commandParameter ?? e);
                if (execute) command.Execute(commandParameter);
            }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(EnterToCommandHelper), new PropertyMetadata(null, OnCommandParameterChanged));
        public static object GetCommandParameter(DependencyObject obj) => obj.GetValue(CommandParameterProperty);

        public static void SetCommandParameter(DependencyObject obj, object value) => obj.SetValue(CommandParameterProperty, value);

        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetCommandParameter(d, e.NewValue);
        }

    }
}
