using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class AutomationRuntimeService : IAutomationRuntimeService
{
    private const string LiveConfirmationPhrase = "PLACE LIVE BETS";
    private readonly object _gate = new();
    private readonly ISiteSessionService _siteSessionService;
    private readonly IStrategySessionService _strategySessionService;
    private readonly IAutomationStateService _automationStateService;
    private readonly ISiteCatalogService _siteCatalogService;
    private readonly IStrategyCatalogService _strategyCatalogService;
    private readonly IBetExecutionService _betExecutionService;
    private readonly IAppSettingsService _settingsService;
    private CancellationTokenSource? _loopCancellation;

    public AutomationRuntimeService(
        ISiteSessionService siteSessionService,
        IStrategySessionService strategySessionService,
        IAutomationStateService automationStateService,
        ISiteCatalogService siteCatalogService,
        IStrategyCatalogService strategyCatalogService,
        IBetExecutionService betExecutionService,
        IAppSettingsService settingsService)
    {
        _siteSessionService = siteSessionService;
        _strategySessionService = strategySessionService;
        _automationStateService = automationStateService;
        _siteCatalogService = siteCatalogService;
        _strategyCatalogService = strategyCatalogService;
        _betExecutionService = betExecutionService;
        _settingsService = settingsService;
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

        var settings = LoadSettings();
        if (!settings.EnableAutomationLoop)
        {
            return new AutomationCommandResult(false, "Automation loop is disabled in settings.");
        }

        var isLiveMode = string.Equals(siteState.Mode, "Live", StringComparison.OrdinalIgnoreCase);
        if (isLiveMode)
        {
            if (!settings.AllowLiveBetExecution
                || !string.Equals(settings.LiveBetConfirmationPhrase, LiveConfirmationPhrase, StringComparison.Ordinal))
            {
                return new AutomationCommandResult(false, "Live bet execution is locked. Enable it in settings and enter the exact confirmation phrase.");
            }

            return new AutomationCommandResult(false, "Live bet execution is gated but not yet enabled in this build. Run simulation loop first.");
        }

        lock (_gate)
        {
            if (_loopCancellation is not null)
            {
                return new AutomationCommandResult(false, "Automation loop is already running.");
            }

            _loopCancellation = new CancellationTokenSource();
            _automationStateService.Start("Simulation", $"Simulation loop started for {siteInstance.SiteName} using {strategyInstance.StrategyName}.");
            _ = Task.Run(() => RunSimulationLoopAsync(settings, _loopCancellation.Token));
        }

        return new AutomationCommandResult(true, $"Simulation loop started for {siteInstance.SiteName} using {strategyInstance.StrategyName}.");
    }

    public AutomationCommandResult Pause()
    {
        _automationStateService.Pause();
        return new AutomationCommandResult(true, "Runtime paused.");
    }

    public AutomationCommandResult Stop()
    {
        lock (_gate)
        {
            _loopCancellation?.Cancel();
            _loopCancellation?.Dispose();
            _loopCancellation = null;
        }

        _automationStateService.Stop();
        return new AutomationCommandResult(true, "Runtime stopped.");
    }

    private async Task RunSimulationLoopAsync(NativeUiSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (string.Equals(_automationStateService.Current.Status, "Paused", StringComparison.OrdinalIgnoreCase))
                {
                    await Task.Delay(GetDelay(settings), cancellationToken);
                    continue;
                }

                var result = _betExecutionService.PrepareNextBet(out var preview);
                _automationStateService.RecordIteration(result.Message, preview);

                if (settings.AutomationMaxSimulationIterations > 0
                    && _automationStateService.Current.LoopIterations >= settings.AutomationMaxSimulationIterations)
                {
                    _automationStateService.Complete("Simulation iteration limit reached.");
                    break;
                }

                if (!result.Succeeded)
                {
                    _automationStateService.Complete(result.Message);
                    break;
                }

                await Task.Delay(GetDelay(settings), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            lock (_gate)
            {
                _loopCancellation?.Dispose();
                _loopCancellation = null;
            }
        }
    }

    private NativeUiSettings LoadSettings()
    {
        try
        {
            return _settingsService.LoadAsync().GetAwaiter().GetResult();
        }
        catch
        {
            return new NativeUiSettings();
        }
    }

    private static int GetDelay(NativeUiSettings settings)
    {
        return Math.Clamp(settings.AutomationLoopDelayMs, 100, 60_000);
    }
}
