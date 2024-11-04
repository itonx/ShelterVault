using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.System;

namespace ShelterVault.Shared.Behaviors
{
    public class KeyDownToCommandBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(KeyDownToCommandBehavior),
                new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                nameof(CommandParameter),
                typeof(object),
                typeof(KeyDownToCommandBehavior),
                new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty VirtualKeyTriggerProperty =
            DependencyProperty.Register(
                nameof(VirtualKeyTrigger),
                typeof(VirtualKey),
                typeof(KeyDownToCommandBehavior),
                new PropertyMetadata(null));

        public VirtualKey VirtualKeyTrigger
        {
            get { return (VirtualKey)GetValue(VirtualKeyTriggerProperty); }
            set { SetValue(VirtualKeyTriggerProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
        }

        private void AssociatedObject_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKeyTrigger)
            {
                bool canExecute = this.Command?.CanExecute(this.CommandParameter ?? e) ?? false;
                if (canExecute) this.Command?.Execute(this.CommandParameter ?? e);
            }
        }
    }
}
