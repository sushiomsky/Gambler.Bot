using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class JsonAppSettingsServiceTests : IDisposable
{
    private readonly string _settingsPath = Path.Combine(
        Path.GetTempPath(),
        $"gambler-bot-settings-{Guid.NewGuid():N}",
        "WinUISettings.json");

    [Fact]
    public async Task MissingFileReturnsDefaultSettings()
    {
        var service = CreateService();

        var settings = await service.LoadAsync();

        Assert.True(settings.UseNativeTheme);
        Assert.True(settings.PromptBeforeUpdates);
        Assert.True(settings.RiskGuardEnabled);
        Assert.True(settings.SessionInsightsEnabled);
        Assert.Equal("DECOY", settings.DefaultCurrency);
        Assert.Equal("Dice", settings.DefaultGame);
        Assert.Equal(0.01m, settings.MinimumBetAmount);
        Assert.Equal(1, settings.MaximumLiveBetsPerRun);
        Assert.Equal(0.05m, settings.LiveStopLossAmount);
        Assert.Equal(0.05m, settings.LiveTakeProfitAmount);
        Assert.True(settings.RequireDecoyCurrencyForLiveBets);
        Assert.False(settings.EnableLiveAutomationLoop);
        Assert.Equal("SQLite", settings.DefaultStorageProvider);
    }

    [Fact]
    public async Task SaveAndLoadRoundTripsSettings()
    {
        var service = CreateService();
        var settings = new NativeUiSettings
        {
            UseNativeTheme = false,
            PromptBeforeUpdates = false,
            RiskGuardEnabled = false,
            SessionInsightsEnabled = false,
            EnableLiveAutomationLoop = true,
            DefaultSite = "DuckDice",
            DefaultCurrency = "decoy",
            DefaultGame = "Dice",
            MinimumBetAmount = 0.01m,
            MaximumLiveBetAmount = 0.02m,
            MaximumLiveBetsPerRun = 3,
            LiveStopLossAmount = 0.25m,
            LiveTakeProfitAmount = 0.5m,
            RequireDecoyCurrencyForLiveBets = true,
            BetHistoryPageSize = 500,
            ConsoleRetentionEntries = 750,
            ChartMaximumPoints = 200,
            DefaultStorageProvider = "PostgreSQL"
        };

        await service.SaveAsync(settings);
        var loaded = await service.LoadAsync();

        Assert.False(loaded.UseNativeTheme);
        Assert.False(loaded.PromptBeforeUpdates);
        Assert.False(loaded.RiskGuardEnabled);
        Assert.False(loaded.SessionInsightsEnabled);
        Assert.True(loaded.EnableLiveAutomationLoop);
        Assert.Equal("DuckDice", loaded.DefaultSite);
        Assert.Equal("DECOY", loaded.DefaultCurrency);
        Assert.Equal("Dice", loaded.DefaultGame);
        Assert.Equal(0.01m, loaded.MinimumBetAmount);
        Assert.Equal(0.02m, loaded.MaximumLiveBetAmount);
        Assert.Equal(3, loaded.MaximumLiveBetsPerRun);
        Assert.Equal(0.25m, loaded.LiveStopLossAmount);
        Assert.Equal(0.5m, loaded.LiveTakeProfitAmount);
        Assert.True(loaded.RequireDecoyCurrencyForLiveBets);
        Assert.Equal(500, loaded.BetHistoryPageSize);
        Assert.Equal(750, loaded.ConsoleRetentionEntries);
        Assert.Equal(200, loaded.ChartMaximumPoints);
        Assert.Equal("PostgreSQL", loaded.DefaultStorageProvider);
    }

    [Fact]
    public async Task InvalidJsonReturnsDefaultSettings()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
        await File.WriteAllTextAsync(_settingsPath, "{ broken json");
        var service = CreateService();

        var settings = await service.LoadAsync();

        Assert.True(settings.RiskGuardEnabled);
        Assert.Equal("SQLite", settings.DefaultStorageProvider);
    }

    public void Dispose()
    {
        var directory = Path.GetDirectoryName(_settingsPath);
        if (directory is not null && Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private JsonAppSettingsService CreateService()
    {
        return new JsonAppSettingsService(
            NullLogger<JsonAppSettingsService>.Instance,
            new SettingsValidationService(),
            _settingsPath);
    }
}
