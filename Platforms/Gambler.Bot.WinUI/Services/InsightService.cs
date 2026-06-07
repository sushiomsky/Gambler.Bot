using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class InsightService : IInsightService
{
    private readonly IAppSettingsService _settingsService;
    private readonly ISiteSessionService _siteSessionService;
    private readonly ISiteCatalogService _siteCatalogService;
    private readonly IStrategyCatalogService _strategyCatalogService;
    private readonly IStrategySessionService _strategySessionService;
    private readonly IBetHistoryService _betHistoryService;
    private readonly IAutomationStateService _automationStateService;

    public InsightService(
        IAppSettingsService settingsService,
        ISiteSessionService siteSessionService,
        ISiteCatalogService siteCatalogService,
        IStrategyCatalogService strategyCatalogService,
        IStrategySessionService strategySessionService,
        IBetHistoryService betHistoryService,
        IAutomationStateService automationStateService)
    {
        _settingsService = settingsService;
        _siteSessionService = siteSessionService;
        _siteCatalogService = siteCatalogService;
        _strategyCatalogService = strategyCatalogService;
        _strategySessionService = strategySessionService;
        _betHistoryService = betHistoryService;
        _automationStateService = automationStateService;
    }

    public async Task<IReadOnlyList<InsightItem>> GetInsightsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _settingsService.LoadAsync(cancellationToken);
        var activeSite = _siteSessionService.Current.SelectedSite;
        var sites = _siteCatalogService.GetSites();
        var strategies = _strategyCatalogService.GetStrategies();
        var history = _betHistoryService.GetRecent(activeSite?.Name);

        var insights = new List<InsightItem>
        {
            new("Site catalog", $"{sites.Count} enabled sites discovered from Core.", "Info"),
            new("Strategy catalog", $"{strategies.Count} strategies available, including {strategies.Count(s => s.Kind == "Programmer Mode")} script runtimes.", "Info")
        };

        insights.Add(activeSite is null
            ? new InsightItem("No active site", "Select or simulate a site before starting a session.", "Warning")
            : new InsightItem("Active site", $"{activeSite.Name} is active in {_siteSessionService.Current.Mode.ToLowerInvariant()} mode.", "Success"));

        insights.Add(settings.RiskGuardEnabled
            ? new InsightItem("Risk guard", "Risk guard is enabled for the native client.", "Success")
            : new InsightItem("Risk guard disabled", "Risk warnings are disabled in native settings.", "Warning"));

        insights.Add(_strategySessionService.Current.SelectedStrategy is null
            ? new InsightItem("No active strategy", "Select a strategy before starting automation.", "Warning")
            : new InsightItem("Active strategy", $"{_strategySessionService.Current.SelectedStrategy.Name} is selected.", "Success"));

        insights.Add(new InsightItem(
            "Automation",
            $"Runtime state is {_automationStateService.Current.Status.ToLowerInvariant()}.",
            _automationStateService.Current.Status == "Running" ? "Success" : "Info"));

        insights.Add(history.Count == 0
            ? new InsightItem("No recent bets", "The history surface is ready; persisted bet storage is not connected yet.", "Info")
            : new InsightItem("Recent bets", $"{history.Count} recent records are available for review.", "Success"));

        return insights;
    }
}
