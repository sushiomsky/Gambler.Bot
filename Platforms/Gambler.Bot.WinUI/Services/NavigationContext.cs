namespace Gambler.Bot.WinUI.Services;

public sealed record NavigationContext(
    IAppSettingsService SettingsService,
    IUpdateService UpdateService,
    ISiteCatalogService SiteCatalogService,
    ISiteSessionService SiteSessionService,
    IStrategyCatalogService StrategyCatalogService,
    IStrategySessionService StrategySessionService,
    IStrategyScriptService StrategyScriptService,
    IBetHistoryService BetHistoryService,
    IBetHistoryExportService BetHistoryExportService,
    IBetHistoryFilterService BetHistoryFilterService,
    IBetHistorySummaryService BetHistorySummaryService,
    IBetHistoryDetailService BetHistoryDetailService,
    IBetChartService BetChartService,
    IConsoleLogService ConsoleLogService,
    IRollVerifierService RollVerifierService,
    IInsightService InsightService,
    IAutomationStateService AutomationStateService,
    IAutomationRuntimeService AutomationRuntimeService,
    ILoginPreparationService LoginPreparationService,
    ILiveLoginService LiveLoginService,
    IBetExecutionService BetExecutionService);
