using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Crash;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Games.Limbo;
using Gambler.Bot.Common.Games.Twist;
using Gambler.Bot.WinUI.Models;
using System.Globalization;

namespace Gambler.Bot.WinUI.Services;

public sealed class RollVerifierService : IRollVerifierService
{
    private readonly ISiteCatalogService _siteCatalogService;

    public RollVerifierService(ISiteCatalogService siteCatalogService)
    {
        _siteCatalogService = siteCatalogService;
    }

    public RollVerificationResult Verify(RollVerificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ServerSeed))
        {
            return Failed(request, "Server seed is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ClientSeed))
        {
            return Failed(request, "Client seed is required.");
        }

        if (!Enum.TryParse<Games>(request.Game, ignoreCase: true, out var game))
        {
            return Failed(request, $"{request.Game} is not a supported game.");
        }

        var site = _siteCatalogService.CreateSite(request.Site);
        if (site is null)
        {
            return Failed(request, $"{request.Site.Name} could not be created from Core.");
        }

        if (!site.CanVerify)
        {
            return Failed(request, $"{site.SiteName} does not advertise roll verification support.");
        }

        try
        {
            var lucky = site.GetLucky(request.ServerSeed.Trim(), request.ClientSeed.Trim(), request.Nonce, game);
            if (lucky is null)
            {
                return Failed(request, "The site returned no verification result.");
            }

            var (type, value) = Describe(lucky);
            return new RollVerificationResult(
                true,
                "Roll verified.",
                site.SiteName,
                game.ToString(),
                site.GetHash(request.ServerSeed.Trim()),
                type,
                value);
        }
        catch (Exception ex)
        {
            return Failed(request, $"Verification failed: {ex.Message}");
        }
    }

    private static RollVerificationResult Failed(RollVerificationRequest request, string message)
    {
        return new RollVerificationResult(false, message, request.Site.Name, request.Game, string.Empty, string.Empty, string.Empty);
    }

    private static (string Type, string Value) Describe(IGameResult result)
    {
        return result switch
        {
            DiceResult dice => ("Dice roll", dice.Roll.ToString("0.####", CultureInfo.InvariantCulture)),
            TwistResult twist => ("Twist roll", twist.Roll.ToString("0.####", CultureInfo.InvariantCulture)),
            LimboResult limbo => ("Limbo result", $"{limbo.Result.ToString("0.####", CultureInfo.InvariantCulture)}x"),
            CrashResult crash => ("Crash result", $"{crash.Result.ToString("0.####", CultureInfo.InvariantCulture)}x"),
            _ => (result.Game.ToString(), result.ToString() ?? string.Empty)
        };
    }
}
