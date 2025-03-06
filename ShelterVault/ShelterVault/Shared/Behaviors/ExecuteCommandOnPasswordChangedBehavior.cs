using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace ShelterVault.Shared.Behaviors
{
    public class ExecuteCommandOnPasswordChangedBehavior : Behavior<PasswordBox>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(ExecuteCommandOnPasswordChangedBehavior),
                new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += PasswordBox_Loaded;
            AssociatedObject.PasswordChanged += PasswordBox_PasswordChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= PasswordBox_Loaded;
            AssociatedObject.PasswordChanged -= PasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.Command?.Execute(this.AssociatedObject?.Password ?? null);
        }

        private void PasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            this.Command?.Execute(this.AssociatedObject?.Password ?? null);
        }
    }
}
