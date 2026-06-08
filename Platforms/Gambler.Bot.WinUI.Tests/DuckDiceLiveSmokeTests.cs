using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Sites.Classes;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Abstractions;

namespace Gambler.Bot.WinUI.Tests;

public sealed class DuckDiceLiveSmokeTests
{
    private const string Currency = "DECOY";
    private const decimal BetAmount = 0.01m;
    private const decimal Chance = 49.5m;
    private const string LiveBetConfirmation = "DECOY_0.01_OK";
    private readonly ITestOutputHelper _output;

    public DuckDiceLiveSmokeTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DuckDiceDecoyMinimumBetCanBePlacedWhenExplicitlyEnabled()
    {
        var apiKey = LoadApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _output.WriteLine("Skipped live DuckDice test: no API key configured.");
            return;
        }

        var confirmation = Environment.GetEnvironmentVariable("GAMBLER_BOT_ENABLE_DUCKDICE_LIVE_BET");
        if (!string.Equals(confirmation, LiveBetConfirmation, StringComparison.Ordinal))
        {
            _output.WriteLine($"Skipped live DuckDice test: set GAMBLER_BOT_ENABLE_DUCKDICE_LIVE_BET={LiveBetConfirmation} to place exactly one {BetAmount:0.00} {Currency} bet.");
            return;
        }

        var site = new DuckDice(NullLogger.Instance)
        {
            CurrentCurrency = Currency
        };

        var loggedIn = await site.LogIn(site.Mirrors[0], CreateLogin(apiKey));
        Assert.True(loggedIn);
        Assert.True(site.Stats.Balance >= BetAmount, $"DuckDice {Currency} balance must be at least {BetAmount:0.00}.");

        var bet = await site.PlaceDiceBet(new PlaceDiceBet(BetAmount, true, Chance));

        Assert.NotNull(bet);
        Assert.Equal(Currency, bet.Currency);
        Assert.Equal(BetAmount, bet.TotalAmount);
        Assert.Equal(Chance, bet.Chance);
        Assert.False(string.IsNullOrWhiteSpace(bet.BetID));
        _output.WriteLine($"Placed DuckDice live smoke bet {bet.BetID}: {bet.TotalAmount:0.00} {bet.Currency}, profit {bet.Profit:0.########}.");
    }

    private static LoginParamValue[] CreateLogin(string apiKey)
    {
        return
        [
            new LoginParamValue
            {
                ParameterId = 0,
                Param = new LoginParameter("API Key", true, true, false, true),
                Value = apiKey
            }
        ];
    }

    private static string? LoadApiKey()
    {
        var environmentValue = Environment.GetEnvironmentVariable("GAMBLER_BOT_DUCKDICE_API_KEY");
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue.Trim();
        }

        foreach (var path in GetSecretFileCandidates())
        {
            if (File.Exists(path))
            {
                var value = File.ReadAllText(path).Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> GetSecretFileCandidates()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrWhiteSpace(appData))
        {
            yield return Path.Combine(appData, "Gambler.Bot", "secrets", "duckdice-api-key.txt");
        }

        yield return Path.Combine(AppContext.BaseDirectory, ".secrets", "duckdice-api-key.txt");
        yield return Path.Combine(Directory.GetCurrentDirectory(), ".secrets", "duckdice-api-key.txt");
    }
}
