using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShelterVault.Tools
{
    public static class ItemClickToCommandHelper
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ItemClickToCommandHelper), new PropertyMetadata(null));

        public static ICommand GetCommand(DependencyObject obj) => (ICommand)obj.GetValue(CommandProperty);

        public static void SetCommand(DependencyObject obj, ICommand value) => obj.SetValue(CommandProperty, value);

        public static readonly DependencyProperty EnableItemClickProperty =
            DependencyProperty.RegisterAttached("EnableItemClick", typeof(bool), typeof(ItemClickToCommandHelper), new PropertyMetadata(false, OnEnableItemClickChanged));

        public static bool GetEnableItemClick(DependencyObject obj) => (bool)obj.GetValue(EnableItemClickProperty);

        public static void SetEnableItemClick(DependencyObject obj, bool value) => obj.SetValue(EnableItemClickProperty, value);

        private static void OnEnableItemClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView && e.NewValue is bool val && val)
            {
                ListView control = d as ListView;
                control.IsItemClickEnabled = true;
                d.SetValue(CommandProperty, e.NewValue);
                control.SelectionChanged += Control_SelectionChanged;
            }
        }

        private static void Control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView control && control.SelectedItem != null)
            {
                ListViewItem selectedItem = control.SelectedItem as ListViewItem;
                control.SelectedIndex = -1;
                ICommand command = GetCommand(selectedItem);
                object commandParameter = GetCommandParameter(selectedItem);
                bool execute = command.CanExecute(commandParameter ?? e);
                if (execute) command.Execute(commandParameter);
            }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(ItemClickToCommandHelper), new PropertyMetadata(null, OnCommandParameterChanged));
        public static object GetCommandParameter(DependencyObject obj) => obj.GetValue(CommandParameterProperty);

        public static void SetCommandParameter(DependencyObject obj, object value) => obj.SetValue(CommandParameterProperty, value);

        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetCommandParameter(d, e.NewValue);
        }
    }
}
