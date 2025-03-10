using CommunityToolkit.Mvvm.Messaging;
using Desktiny.UI.Tools;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using ShelterVault.Shared.Constants;
using ShelterVault.Shared.Enums;
using ShelterVault.Shared.Extensions;
using ShelterVault.Shared.Helpers;
using ShelterVault.Shared.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;

namespace ShelterVault.Shared.Behaviors
{
    public class ComboBoxChangeLanguageBehavior : Behavior<ComboBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            SetDefaultLanguage();
            AssociatedObject.IsDropDownOpen = true;
            string settingsLang = GetLanguageFromSettings().GetAttribute<DescriptionAttribute>().Description;
            AssociatedObject.SelectedItem = AssociatedObject.Items.FirstOrDefault(i => (i as ComboBoxItem).Tag.Equals(settingsLang));
            AssociatedObject.IsDropDownOpen = false;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string settingsLang = GetLanguageFromSettings().GetAttribute<DescriptionAttribute>().Description;
            string selectedLang = (AssociatedObject.SelectedItem as ComboBoxItem).Tag.ToString();

            if (settingsLang.Equals(selectedLang, StringComparison.InvariantCultureIgnoreCase)) return;

            Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = selectedLang;
            ResourceManager.Current.DefaultContext.QualifierValues["Language"] = selectedLang;
            SaveLanguageSettings(selectedLang);
            RefreshUI();
            WeakReferenceMessenger.Default.Send(new UpdateLanguageValuesMessage(GetLanguageFromSettings()));
        }

        private void RefreshUI()
        {
            if (WindowHelper.CurrentMainWindow?.Content == null) return;
            Frame navigator = WinUI3Helper.FindChildElementByName(WindowHelper.CurrentMainWindow.Content, "Navigator") as Frame;
            navigator.Navigate(navigator.Content.GetType());
        }

        private ShelterVaultLang GetLanguageFromSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string lang = localSettings.Values[ShelterVaultConstants.SETTINGS_LANG_KEY] as string;

            Enum.TryParse(typeof(ShelterVaultLang), lang, true, out object shelterVaultLangObj);
            return (ShelterVaultLang)shelterVaultLangObj;
        }

        private void SaveLanguageSettings(ShelterVaultLang shelterVaultLang)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values[ShelterVaultConstants.SETTINGS_LANG_KEY] = shelterVaultLang.ToString();
        }

        private void SaveLanguageSettings(string shelterVaultLang)
        {
            if (shelterVaultLang == null) return;
            IEnumerable<ShelterVaultLang> languagesAvailable = Enum.GetValues(typeof(ShelterVaultLang)).OfType<ShelterVaultLang>();
            ShelterVaultLang langSetting = languagesAvailable.FirstOrDefault(l => l.GetAttribute<DescriptionAttribute>().Description.Equals(shelterVaultLang));
            SaveLanguageSettings(langSetting);
        }

        private void SetDefaultLanguage()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string lang = localSettings.Values[ShelterVaultConstants.SETTINGS_LANG_KEY] as string;
            Enum.TryParse(typeof(ShelterVaultLang), lang, true, out object shelterVaultLangObj);
            if (shelterVaultLangObj != null && ExistsInAppManifest((ShelterVaultLang)shelterVaultLangObj)) return;

            string defaultLangOverride = Microsoft.Windows.Globalization.ApplicationLanguages.Languages.FirstOrDefault();

            if (ExistsInAppManifest(defaultLangOverride))
            {
                SaveLanguageSettings(defaultLangOverride);
                return;
            }

            SaveLanguageSettings(ShelterVaultLang.English);
        }

        private bool ExistsInAppManifest(string lang)
        {
            return lang != null && Windows.Globalization.ApplicationLanguages.ManifestLanguages.Any(l => l.Equals(lang, StringComparison.InvariantCultureIgnoreCase));
        }

        private bool ExistsInAppManifest(ShelterVaultLang shelterVaultLang)
        {
            string lang = shelterVaultLang.GetAttribute<DescriptionAttribute>().Description;
            return ExistsInAppManifest(lang);
        }
    }
}
