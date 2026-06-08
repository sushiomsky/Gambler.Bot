using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class StrategyScriptServiceTests : IDisposable
{
    private readonly string _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"gambler-bot-script-tests-{Guid.NewGuid():N}");

    [Fact]
    public void PresetStrategiesAreReadOnly()
    {
        var service = CreateService();

        Assert.False(service.CanEdit(TestData.PresetStrategy));
    }

    [Fact]
    public async Task OpenOrCreateCreatesProgrammerModeTemplate()
    {
        var service = CreateService();

        var document = await service.OpenOrCreateAsync(TestData.LuaStrategy);

        Assert.True(File.Exists(document.FilePath));
        Assert.Equal("lua", document.FileExtension);
        Assert.Contains("function dobet", document.Content);
        Assert.Contains(_temporaryDirectory, document.FilePath);
    }

    [Fact]
    public async Task SavePersistsUpdatedScriptContent()
    {
        var service = CreateService();
        var document = await service.OpenOrCreateAsync(TestData.JavaScriptStrategy);
        const string updatedContent = "function dobet(previousBet, win, nextBet) { nextBet.Amount = 0.02; }";

        var saved = await service.SaveAsync(document, updatedContent);

        Assert.Equal(updatedContent, File.ReadAllText(saved.FilePath));
        Assert.Equal(updatedContent, saved.Content);
    }

    [Fact]
    public async Task ValidateRequiresRuntimeEntryPoint()
    {
        var service = CreateService();
        var document = await service.OpenOrCreateAsync(TestData.PythonStrategy);

        var invalid = service.Validate(document with { Content = "print('missing entry point')" });
        var valid = service.Validate(document with { Content = "def dobet(previous_bet, win, next_bet): pass" });

        Assert.False(invalid.Succeeded);
        Assert.Equal("Script should define the dobet entry point.", invalid.Message);
        Assert.True(valid.Succeeded);
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(_temporaryDirectory, recursive: true);
        }
    }

    private StrategyScriptService CreateService()
    {
        return new StrategyScriptService(new EmptyStrategyCatalogService(), _temporaryDirectory);
    }

    private static class TestData
    {
        public static readonly StrategySummary PresetStrategy = new(
            "Martingale",
            "Preset",
            "Native",
            "Available",
            "Test.Martingale");

        public static readonly StrategySummary LuaStrategy = new(
            "Programmer LUA",
            "Programmer Mode",
            "Lua",
            "Available",
            "Test.ProgrammerLUA");

        public static readonly StrategySummary JavaScriptStrategy = new(
            "Programmer JS",
            "Programmer Mode",
            "JavaScript",
            "Available",
            "Test.ProgrammerJS");

        public static readonly StrategySummary PythonStrategy = new(
            "Programmer Python",
            "Programmer Mode",
            "Python",
            "Available",
            "Test.ProgrammerPython");
    }

    private sealed class EmptyStrategyCatalogService : IStrategyCatalogService
    {
        public IReadOnlyList<StrategySummary> GetStrategies()
        {
            return [];
        }

        public BaseStrategy? CreateStrategy(StrategySummary summary)
        {
            return null;
        }
    }
}
