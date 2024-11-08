﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using ShelterVault.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShelterVault.Shared.Helpers;
using ShelterVault.Shared.Constants;

namespace ShelterVault.Services
{
    public interface IDialogService
    {
        Task ShowConfirmationDialogAsync(string title, string message, string primaryButtonText = "Close");
        Task<bool> ShowContinueConfirmationDialogAsync(string title, string message, string primaryButtonText = "No", string secondaryButtonText = "Yes", ContentDialogResult expectedResult = ContentDialogResult.Primary);
    }
    public class DialogService : IDialogService
    {
        public async Task ShowConfirmationDialogAsync(string title, string message, string primaryButtonText = "Close")
        {
            ContentDialog dialog = BuildDialog(title, message, primaryButtonText);
            await dialog.ShowAsync();
        }

        public async Task<bool> ShowContinueConfirmationDialogAsync(string title, string message, string primaryButtonText = "No", string secondaryButtonText = "Yes", ContentDialogResult expectedResult = ContentDialogResult.Primary)
        {
            ContentDialog dialog = BuildDialog(title, message, primaryButtonText);
            dialog.SecondaryButtonText = secondaryButtonText;
            ContentDialogResult result = await dialog.ShowAsync();
            return result == expectedResult;
        }

        private ContentDialog BuildDialog(string title, string message, string primaryButtonText = "Close")
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = WindowHelper.CurrentMainWindow.Content.XamlRoot;
            dialog.RequestedTheme = (WindowHelper.CurrentMainWindow.Content as FrameworkElement).RequestedTheme;
            dialog.Style = Application.Current.Resources[ShelterVaultConstants.DIALOG_STYLE_KEY] as Style;
            dialog.Title = title;
            dialog.PrimaryButtonText = primaryButtonText;
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new ShelterVaultMessageView(message);

            return dialog;
        }
    }
}
