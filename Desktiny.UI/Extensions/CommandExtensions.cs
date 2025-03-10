﻿using Microsoft.UI.Xaml;
using System.Windows.Input;

namespace ShelterVault.Shared.Extensions
{
    public static class CommandExtensions
    {
        public static void ExecuteAttachedCommand(this ICommand command, DependencyProperty dependencyProperty, object parameter = null)
        {
            command?.Execute(parameter);
        }
    }
}
