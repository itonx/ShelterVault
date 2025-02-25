﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using ShelterVault.DataLayer;
using ShelterVault.Managers;
using ShelterVault.Services;
using ShelterVault.ViewModels;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ShelterVault
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Services = ConfigureServices();
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            AppDispatcher.UIThreadDispatcher = DispatcherQueue.GetForCurrentThread();
            m_window.Activate();
        }

        /// <summary>
        /// Gets the current <see cref="Window"/>
        /// </summary>
        internal Window m_window;

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            //LocalDb
            services.AddSingleton<IShelterVaultLocalDb, ShelterVaultLocalDb>();
            services.AddSingleton<IShelterVault, DataLayer.ShelterVault>();
            services.AddSingleton<IShelterVaultCredentials, ShelterVaultCredentials>();
            services.AddSingleton<IShelterVaultCloudConfig, ShelterVaultCloudConfig>();
            services.AddSingleton<IShelterVaultSyncStatus, ShelterVaultSyncStatus>();

            // Services
            services.AddSingleton<ILanguageService, LanguageService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IProgressBarService, ProgressBarService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IShelterVaultStateService, ShelterVaultStateService>();
            services.AddSingleton<IShelterVaultThemeService, ShelterVaultThemeService>();
            services.AddSingleton<IShelterVaultCosmosDBService, ShelterVaultCosmosDBService>();
            services.AddSingleton<IUIThreadService, UIThreadService>();
            services.AddSingleton<IWeakReferenceInstanceManager, WeakReferenceInstanceManager>();
            services.AddSingleton<ICloudProviderManager, CloudProviderManager>();
            services.AddSingleton<MainWindowViewModel>();

            // Managers
            services.AddScoped<IVaultManager, VaultManager>();
            services.AddScoped<ICredentialsManager, CredentialsManager>();
            services.AddScoped<ICloudSyncManager, CloudSyncManager>();

            // Viewmodels
            services.AddTransient<NavigationViewModel>();
            services.AddTransient<PasswordConfirmationViewModel>();
            services.AddTransient<CredentialsViewModel>();
            services.AddTransient<CreateMasterKeyViewModel>();
            services.AddTransient<ConfirmMasterKeyViewModel>();
            services.AddTransient<SettingsViewModel>();

            return services.BuildServiceProvider();
        }
    }

    public static class AppDispatcher
    {
        public static DispatcherQueue UIThreadDispatcher { get; set; }
    }
}
