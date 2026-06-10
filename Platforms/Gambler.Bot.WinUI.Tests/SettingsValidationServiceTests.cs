using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class SettingsValidationServiceTests
{
    [Fact]
    public void NormalizeFillsTextDefaults()
    {
        var service = new SettingsValidationService();
        var settings = new NativeUiSettings
        {
            DefaultStorageProvider = " ",
            DefaultCurrency = " decoy ",
            DefaultGame = " ",
            DefaultSite = " DuckDice "
        };

        var normalized = service.Normalize(settings);

        Assert.Equal("SQLite", normalized.DefaultStorageProvider);
        Assert.Equal("DECOY", normalized.DefaultCurrency);
        Assert.Equal("Dice", normalized.DefaultGame);
        Assert.Equal("DuckDice", normalized.DefaultSite);
    }

    [Fact]
    public void NormalizeClampsAutomationAndLiveSafetyRanges()
    {
        var service = new SettingsValidationService();
        var settings = new NativeUiSettings
        {
            AutomationLoopDelayMs = 1,
            AutomationMaxSimulationIterations = -10,
            MinimumBetAmount = -1m,
            MaximumLiveBetAmount = 0m,
            MaximumLiveBetsPerRun = 0,
            BetHistoryPageSize = 1,
            ConsoleRetentionEntries = 1,
            ChartMaximumPoints = 1
        };

        var normalized = service.Normalize(settings);

        Assert.Equal(100, normalized.AutomationLoopDelayMs);
        Assert.Equal(0, normalized.AutomationMaxSimulationIterations);
        Assert.Equal(0.00000001m, normalized.MinimumBetAmount);
        Assert.Equal(normalized.MinimumBetAmount, normalized.MaximumLiveBetAmount);
        Assert.Equal(1, normalized.MaximumLiveBetsPerRun);
        Assert.Equal(25, normalized.BetHistoryPageSize);
        Assert.Equal(50, normalized.ConsoleRetentionEntries);
        Assert.Equal(10, normalized.ChartMaximumPoints);
    }

    [Fact]
    public void NormalizeTrimsConfirmationPhrase()
    {
        var service = new SettingsValidationService();

        var normalized = service.Normalize(new NativeUiSettings
        {
            LiveBetConfirmationPhrase = " PLACE LIVE BETS "
        });

        Assert.Equal("PLACE LIVE BETS", normalized.LiveBetConfirmationPhrase);
    }
}
