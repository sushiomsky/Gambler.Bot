using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class AutomationRuntimeService : IAutomationRuntimeService
{
    private readonly ISiteSessionService _siteSessionService;
    private readonly IStrategySessionService _strategySessionService;
    private readonly IAutomationStateService _automationStateService;
    private readonly ISiteCatalogService _siteCatalogService;
    private readonly IStrategyCatalogService _strategyCatalogService;

    public AutomationRuntimeService(
        ISiteSessionService siteSessionService,
        IStrategySessionService strategySessionService,
        IAutomationStateService automationStateService,
        ISiteCatalogService siteCatalogService,
        IStrategyCatalogService strategyCatalogService)
    {
        _siteSessionService = siteSessionService;
        _strategySessionService = strategySessionService;
        _automationStateService = automationStateService;
        _siteCatalogService = siteCatalogService;
        _strategyCatalogService = strategyCatalogService;
    }

    public AutomationCommandResult Start()
    {
        var siteState = _siteSessionService.Current;
        var site = siteState.SelectedSite;
        if (site is null)
        {
            return new AutomationCommandResult(false, "Select or simulate a site before starting.");
        }

        if (!siteState.IsConnected)
        {
            return new AutomationCommandResult(false, "Use simulation mode or complete login before starting.");
        }

        var strategy = _strategySessionService.Current.SelectedStrategy;
        if (strategy is null)
        {
            return new AutomationCommandResult(false, "Select a strategy before starting.");
        }

        var siteInstance = _siteCatalogService.CreateSite(site);
        if (siteInstance is null)
        {
            return new AutomationCommandResult(false, $"{site.Name} could not be created from Core.");
        }

        var strategyInstance = _strategyCatalogService.CreateStrategy(strategy);
        if (strategyInstance is null)
        {
            return new AutomationCommandResult(false, $"{strategy.Name} could not be created from Strategies.");
        }

        _automationStateService.Start();
        return new AutomationCommandResult(true, $"Runtime prepared for {siteInstance.SiteName} using {strategyInstance.StrategyName}.");
    }

    public AutomationCommandResult Pause()
    {
        _automationStateService.Pause();
        return new AutomationCommandResult(true, "Runtime paused.");
    }

    public AutomationCommandResult Stop()
    {
        _automationStateService.Stop();
        return new AutomationCommandResult(true, "Runtime stopped.");
    }
}
