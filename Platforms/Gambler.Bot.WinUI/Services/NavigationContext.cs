namespace Gambler.Bot.WinUI.Services;

public sealed record NavigationContext(
    IAppSettingsService SettingsService,
    IUpdateService UpdateService,
    ISiteCatalogService SiteCatalogService,
    ISiteSessionService SiteSessionService,
    IStrategyCatalogService StrategyCatalogService,
    IStrategySessionService StrategySessionService,
    IBetHistoryService BetHistoryService,
    IInsightService InsightService,
    IAutomationStateService AutomationStateService,
    IAutomationRuntimeService AutomationRuntimeService,
    ILoginPreparationService LoginPreparationService,
    ILiveLoginService LiveLoginService,
    IBetExecutionService BetExecutionService);
