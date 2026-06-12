using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Limbo;
using Gambler.Bot.Common.Games.Twist;
using Gambler.Bot.WinUI.Models;
using System.Globalization;

namespace Gambler.Bot.WinUI.Services;

public sealed class BetExecutionService : IBetExecutionService
{
    private const string LiveConfirmationPhrase = "PLACE LIVE BETS";
    private readonly ISiteSessionService _siteSessionService;
    private readonly IStrategySessionService _strategySessionService;
    private readonly ISiteCatalogService _siteCatalogService;
    private readonly IStrategyCatalogService _strategyCatalogService;
    private readonly IAppSettingsService _settingsService;
    private int _liveBetsThisRun;

    public BetExecutionService(
        ISiteSessionService siteSessionService,
        IStrategySessionService strategySessionService,
        ISiteCatalogService siteCatalogService,
        IStrategyCatalogService strategyCatalogService,
        IAppSettingsService settingsService)
    {
        _siteSessionService = siteSessionService;
        _strategySessionService = strategySessionService;
        _siteCatalogService = siteCatalogService;
        _strategyCatalogService = strategyCatalogService;
        _settingsService = settingsService;
    }

    public AutomationCommandResult PrepareNextBet(out PlaceBetPreview? preview)
    {
        preview = null;

        var siteSummary = _siteSessionService.Current.SelectedSite;
        if (siteSummary is null)
        {
            return new AutomationCommandResult(false, "Select or login to a site before preparing a bet.");
        }

        var strategySummary = _strategySessionService.Current.SelectedStrategy;
        if (strategySummary is null)
        {
            return new AutomationCommandResult(false, "Select a strategy before preparing a bet.");
        }

        var site = _siteCatalogService.CreateSite(siteSummary);
        var strategy = _strategyCatalogService.CreateStrategy(strategySummary);
        if (site is null || strategy is null)
        {
            return new AutomationCommandResult(false, "Runtime objects could not be created.");
        }

        var game = site.SupportedGames.FirstOrDefault();
        strategy.Config = site.GetGameSettings(game);

        try
        {
            var nextBet = strategy.Start(game);
            preview = new PlaceBetPreview(
                site.SiteName,
                strategy.StrategyName,
                game.ToString(),
                site.CurrentCurrency,
                nextBet.Amount,
                Describe(nextBet));

            return new AutomationCommandResult(true, "Next bet prepared.");
        }
        catch (Exception ex)
        {
            return new AutomationCommandResult(false, $"Bet preparation failed: {ex.Message}");
        }
    }

    public async Task<AutomationCommandResult> ExecuteLiveBetAsync(CancellationToken cancellationToken = default)
    {
        var siteState = _siteSessionService.Current;
        if (!string.Equals(siteState.Mode, "Live", StringComparison.OrdinalIgnoreCase)
            || !siteState.IsConnected
            || siteState.RuntimeSite is null)
        {
            return new AutomationCommandResult(false, "Complete live login before placing a live bet.");
        }

        if (!siteState.RuntimeSite.LoggedIn)
        {
            return new AutomationCommandResult(false, "The live site session is no longer logged in.");
        }

        var strategySummary = _strategySessionService.Current.SelectedStrategy;
        if (strategySummary is null)
        {
            return new AutomationCommandResult(false, "Select a strategy before placing a live bet.");
        }

        var settings = await _settingsService.LoadAsync(cancellationToken);
        var safety = ValidateLiveSafety(settings, siteState.RuntimeSite.CurrentCurrency);
        if (!safety.Succeeded)
        {
            return safety;
        }

        if (_liveBetsThisRun >= settings.MaximumLiveBetsPerRun)
        {
            return new AutomationCommandResult(false, $"Live bet limit reached ({settings.MaximumLiveBetsPerRun} per run).");
        }

        var strategy = _strategyCatalogService.CreateStrategy(strategySummary);
        if (strategy is null)
        {
            return new AutomationCommandResult(false, $"{strategySummary.Name} could not be created from Strategies.");
        }

        var game = siteState.RuntimeSite.SupportedGames.FirstOrDefault();
        strategy.Config = siteState.RuntimeSite.GetGameSettings(game);

        try
        {
            var nextBet = strategy.Start(game);
            var amountCheck = ApplyLiveBetAmountSafety(nextBet, settings);
            if (!amountCheck.Succeeded)
            {
                return amountCheck;
            }

            var result = await siteState.RuntimeSite.PlaceBet(nextBet);
            if (result is null)
            {
                return new AutomationCommandResult(false, "Live bet failed: site returned no bet result.");
            }

            _liveBetsThisRun++;
            return new AutomationCommandResult(
                true,
                $"Live bet placed: {result.TotalAmount.ToString("0.########", CultureInfo.InvariantCulture)} {result.Currency}, profit {result.Profit.ToString("0.########", CultureInfo.InvariantCulture)}, id {result.BetID}.",
                result.Profit);
        }
        catch (Exception ex)
        {
            return new AutomationCommandResult(false, $"Live bet failed: {ex.Message}");
        }
    }

    private static AutomationCommandResult ValidateLiveSafety(NativeUiSettings settings, string currency)
    {
        if (!settings.AllowLiveBetExecution
            || !string.Equals(settings.LiveBetConfirmationPhrase, LiveConfirmationPhrase, StringComparison.Ordinal))
        {
            return new AutomationCommandResult(false, "Live bet execution is locked. Enable it in settings and enter the exact confirmation phrase.");
        }

        if (settings.RequireDecoyCurrencyForLiveBets
            && !string.Equals(currency, "DECOY", StringComparison.OrdinalIgnoreCase))
        {
            return new AutomationCommandResult(false, "Live bet execution requires DECOY currency in settings.");
        }

        return new AutomationCommandResult(true, "Live safety checks passed.");
    }

    private static AutomationCommandResult ApplyLiveBetAmountSafety(PlaceBet bet, NativeUiSettings settings)
    {
        if (bet.Amount < settings.MinimumBetAmount)
        {
            bet.Amount = settings.MinimumBetAmount;
        }

        if (bet.Amount > settings.MaximumLiveBetAmount)
        {
            return new AutomationCommandResult(false, $"Live bet amount {bet.Amount.ToString("0.########", CultureInfo.InvariantCulture)} exceeds maximum {settings.MaximumLiveBetAmount.ToString("0.########", CultureInfo.InvariantCulture)}.");
        }

        return new AutomationCommandResult(true, "Bet amount passed live safety checks.");
    }

    private static string Describe(PlaceBet bet)
    {
        return bet switch
        {
            PlaceDiceBet dice => $"{(dice.High ? "High" : "Low")} at {dice.Chance:0.####}%",
            PlaceLimboBet limbo => $"Payout {limbo.Payout:0.####}x",
            PlaceTwistBet twist => $"{(twist.High ? "High" : "Low")} at {twist.Chance:0.####}%",
            PlaceCrashBet crash => $"Payout {crash.Payout:0.####}x",
            _ => bet.Game.ToString()
        };
    }
}
