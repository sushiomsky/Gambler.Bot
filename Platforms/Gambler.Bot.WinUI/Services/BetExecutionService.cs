using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Limbo;
using Gambler.Bot.Common.Games.Twist;
using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class BetExecutionService : IBetExecutionService
{
    private readonly ISiteSessionService _siteSessionService;
    private readonly IStrategySessionService _strategySessionService;
    private readonly ISiteCatalogService _siteCatalogService;
    private readonly IStrategyCatalogService _strategyCatalogService;

    public BetExecutionService(
        ISiteSessionService siteSessionService,
        IStrategySessionService strategySessionService,
        ISiteCatalogService siteCatalogService,
        IStrategyCatalogService strategyCatalogService)
    {
        _siteSessionService = siteSessionService;
        _strategySessionService = strategySessionService;
        _siteCatalogService = siteCatalogService;
        _strategyCatalogService = strategyCatalogService;
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
