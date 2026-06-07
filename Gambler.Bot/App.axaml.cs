using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Gambler.Bot.ViewModels;
using Gambler.Bot.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;
using Gambler.Bot.ViewModels.Common;
using Microsoft.Extensions.Configuration;
using Projektanker.Icons.Avalonia.MaterialDesign;
using Projektanker.Icons.Avalonia;
using Velopack;
using System.Threading.Tasks;
using Serilog;
using Velopack.Sources;
using System.Reflection;
using ActiproSoftware.Extensions;

namespace Gambler.Bot
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDeveloperTools();
#endif

            // Workaround for default ToggleThemeButton theme in Actipro Avalonia v24.1.0
            _ = ActiproSoftware.Properties.Shared.AssemblyInfo.Instance;
            
        }
        internal static async Task<bool> HasUpdate()
        {
            var mgr = new UpdateManager(new GithubSource("https://github.com/Seuntjie900/Gambler.Bot", null, false));

            // check for new version
            var newVersion = await mgr.CheckForUpdatesAsync();
            return newVersion != null;               
        }
        internal static async Task UpdateMyApp()
        {
            var mgr = new UpdateManager(new GithubSource("https://github.com/Seuntjie900/Gambler.Bot", null, false));

            // check for new version
            var newVersion = await mgr.CheckForUpdatesAsync();
            if (newVersion == null)
                return; // no update available

            // download new version
            await mgr.DownloadUpdatesAsync(newVersion);

            // install new version and restart app
            mgr.ApplyUpdatesAndRestart(newVersion);
        }
        internal static string GetVersion()
        {
            try
            {
                var mgr = new UpdateManager(new GithubSource("https://github.com/Seuntjie900/Gambler.Bot", null, false));
                return mgr.CurrentVersion?.ToFullString() ?? GetAssemblyVersion();
            }
            catch (Exception)
            {
                return GetAssemblyVersion();
            }
        }

        private static string GetAssemblyVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString()
                ?? "0.0.0";
        }
        internal static bool IsPortable()
        {
            try
            {
                var mgr = new UpdateManager(new GithubSource("https://github.com/Seuntjie900/Gambler.Bot", null, false));
                return mgr?.IsPortable ?? false;
            }
            catch (Exception ex)
            {
                return false;

            }
        }
        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                //var logger = ServiceProvider.GetService<ILogger<MainWindowViewModel>>();
                desktop.MainWindow = new MainWindow { DataContext = ServiceProvider.GetService<MainWindowViewModel>(), };
            } else if(ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = ServiceProvider.GetService<MainWindowViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
           
            IconProvider.Current.Register<MaterialDesignIconProvider>();
            services.AddLogging(configure => configure.AddSerilog().AddConsole().SetMinimumLevel(LogLevel.Debug).AddDebug());
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<SelectSiteViewModel>();
            // Register other ViewModels and services
        }
    }
}
