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
            DefaultStorageProvider = "PostgreSQL"
        };

        await service.SaveAsync(settings);
        var loaded = await service.LoadAsync();

        Assert.False(loaded.UseNativeTheme);
        Assert.False(loaded.PromptBeforeUpdates);
        Assert.False(loaded.RiskGuardEnabled);
        Assert.False(loaded.SessionInsightsEnabled);
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
        return new JsonAppSettingsService(NullLogger<JsonAppSettingsService>.Instance, _settingsPath);
    }
}
