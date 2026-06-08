using Gambler.Bot.WinUI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Velopack;

namespace Gambler.Bot.WinUI;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        VelopackApp.Build().Run();
        InitializeComponent();
        Services = ConfigureServices();
    }

    public IServiceProvider Services { get; }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = Services.GetRequiredService<MainWindow>();
        _window.Activate();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddDebug());
        services.AddSingleton<IAppSettingsService, JsonAppSettingsService>();
        services.AddSingleton<IUpdateService, VelopackUpdateService>();
        services.AddSingleton<ISiteCatalogService, ReflectionSiteCatalogService>();
        services.AddSingleton<ISiteSessionService, SiteSessionService>();
        services.AddSingleton<IStrategyCatalogService, ReflectionStrategyCatalogService>();
        services.AddSingleton<IStrategySessionService, StrategySessionService>();
        services.AddSingleton<IStrategyScriptService, StrategyScriptService>();
        services.AddSingleton<IBetHistoryService, BetHistoryService>();
        services.AddSingleton<IBetHistoryExportService, BetHistoryExportService>();
        services.AddSingleton<IBetHistoryFilterService, BetHistoryFilterService>();
        services.AddSingleton<IBetHistorySummaryService, BetHistorySummaryService>();
        services.AddSingleton<IBetChartService, BetChartService>();
        services.AddSingleton<IConsoleLogService, ConsoleLogService>();
        services.AddSingleton<IInsightService, InsightService>();
        services.AddSingleton<IAutomationStateService, AutomationStateService>();
        services.AddSingleton<IAutomationRuntimeService, AutomationRuntimeService>();
        services.AddSingleton<ILoginPreparationService, LoginPreparationService>();
        services.AddSingleton<ILiveLoginService, LiveLoginService>();
        services.AddSingleton<IBetExecutionService, BetExecutionService>();
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }
}
