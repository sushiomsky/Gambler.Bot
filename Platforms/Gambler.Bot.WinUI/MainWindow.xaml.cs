using Gambler.Bot.WinUI.Pages;
using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Gambler.Bot.WinUI;

public sealed partial class MainWindow : Window
{
    private readonly IAppSettingsService _settingsService;
    private readonly IUpdateService _updateService;
    private readonly ISiteCatalogService _siteCatalogService;
    private readonly ISiteSessionService _siteSessionService;
    private readonly IStrategyCatalogService _strategyCatalogService;
    private readonly IStrategySessionService _strategySessionService;
    private readonly IStrategyScriptService _strategyScriptService;
    private readonly IBetHistoryService _betHistoryService;
    private readonly IBetHistoryExportService _betHistoryExportService;
    private readonly IBetHistoryFilterService _betHistoryFilterService;
    private readonly IBetHistorySummaryService _betHistorySummaryService;
    private readonly IBetHistoryDetailService _betHistoryDetailService;
    private readonly IBetChartService _betChartService;
    private readonly IConsoleLogService _consoleLogService;
    private readonly IRollVerifierService _rollVerifierService;
    private readonly IInsightService _insightService;
    private readonly IAutomationStateService _automationStateService;
    private readonly IAutomationRuntimeService _automationRuntimeService;
    private readonly ILoginPreparationService _loginPreparationService;
    private readonly ILiveLoginService _liveLoginService;
    private readonly IBetExecutionService _betExecutionService;

    public MainWindow(
        IAppSettingsService settingsService,
        IUpdateService updateService,
        ISiteCatalogService siteCatalogService,
        ISiteSessionService siteSessionService,
        IStrategyCatalogService strategyCatalogService,
        IStrategySessionService strategySessionService,
        IStrategyScriptService strategyScriptService,
        IBetHistoryService betHistoryService,
        IBetHistoryExportService betHistoryExportService,
        IBetHistoryFilterService betHistoryFilterService,
        IBetHistorySummaryService betHistorySummaryService,
        IBetHistoryDetailService betHistoryDetailService,
        IBetChartService betChartService,
        IConsoleLogService consoleLogService,
        IRollVerifierService rollVerifierService,
        IInsightService insightService,
        IAutomationStateService automationStateService,
        IAutomationRuntimeService automationRuntimeService,
        ILoginPreparationService loginPreparationService,
        ILiveLoginService liveLoginService,
        IBetExecutionService betExecutionService)
    {
        _settingsService = settingsService;
        _updateService = updateService;
        _siteCatalogService = siteCatalogService;
        _siteSessionService = siteSessionService;
        _strategyCatalogService = strategyCatalogService;
        _strategySessionService = strategySessionService;
        _strategyScriptService = strategyScriptService;
        _betHistoryService = betHistoryService;
        _betHistoryExportService = betHistoryExportService;
        _betHistoryFilterService = betHistoryFilterService;
        _betHistorySummaryService = betHistorySummaryService;
        _betHistoryDetailService = betHistoryDetailService;
        _betChartService = betChartService;
        _consoleLogService = consoleLogService;
        _rollVerifierService = rollVerifierService;
        _insightService = insightService;
        _automationStateService = automationStateService;
        _automationRuntimeService = automationRuntimeService;
        _loginPreparationService = loginPreparationService;
        _liveLoginService = liveLoginService;
        _betExecutionService = betExecutionService;

        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.SetIcon("Assets/AppIcon.ico");
        NavFrame.Navigate(typeof(HomePage), CreateNavigationContext());
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        NavFrame.GoBack();
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            NavFrame.Navigate(typeof(SettingsPage), CreateNavigationContext());
        }
        else if (args.SelectedItem is NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "home":
                    NavFrame.Navigate(typeof(HomePage), CreateNavigationContext());
                    break;
                case "about":
                    NavFrame.Navigate(typeof(AboutPage));
                    break;
                case "sites":
                    NavFrame.Navigate(typeof(SitesPage), CreateNavigationContext());
                    break;
                case "strategies":
                    NavFrame.Navigate(typeof(StrategiesPage), CreateNavigationContext());
                    break;
                case "login":
                    NavFrame.Navigate(typeof(LoginPage), CreateNavigationContext());
                    break;
                case "history":
                    NavFrame.Navigate(typeof(BetHistoryPage), CreateNavigationContext());
                    break;
                case "console":
                    NavFrame.Navigate(typeof(ConsolePage), CreateNavigationContext());
                    break;
                case "verifier":
                    NavFrame.Navigate(typeof(RollVerifierPage), CreateNavigationContext());
                    break;
                case "intelligence":
                    NavFrame.Navigate(typeof(IntelligencePage), CreateNavigationContext());
                    break;
                default:
                    throw new InvalidOperationException($"Unknown navigation item tag: {item.Tag}");
            }
        }
    }

    private NavigationContext CreateNavigationContext() => new(
        _settingsService,
        _updateService,
        _siteCatalogService,
        _siteSessionService,
        _strategyCatalogService,
        _strategySessionService,
        _strategyScriptService,
        _betHistoryService,
        _betHistoryExportService,
        _betHistoryFilterService,
        _betHistorySummaryService,
        _betHistoryDetailService,
        _betChartService,
        _consoleLogService,
        _rollVerifierService,
        _insightService,
        _automationStateService,
        _automationRuntimeService,
        _loginPreparationService,
        _liveLoginService,
        _betExecutionService);
}
