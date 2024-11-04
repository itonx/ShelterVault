﻿using Microsoft.UI.Xaml;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ShelterVault.Shared.Behaviors
{
    public class FocusBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty OnEventProperty =
            DependencyProperty.Register(
                nameof(OnEvent),
                typeof(string),
                typeof(FocusBehavior),
                new PropertyMetadata(null));

        public string OnEvent
        {
            get { return (string)GetValue(OnEventProperty); }
            set { SetValue(OnEventProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            EventInfo eventInfo = AssociatedObject.GetType().GetEvent(OnEvent);
            if(eventInfo != null) eventInfo.AddEventHandler(AssociatedObject, (RoutedEventHandler)Control_Loaded);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            EventInfo eventInfo = AssociatedObject.GetType().GetEvent(OnEvent);
            if (eventInfo != null) eventInfo.RemoveEventHandler(AssociatedObject, (RoutedEventHandler)Control_Loaded);
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement control)
            {
                control.Focus(FocusState.Programmatic);
            }
        }
    }
}