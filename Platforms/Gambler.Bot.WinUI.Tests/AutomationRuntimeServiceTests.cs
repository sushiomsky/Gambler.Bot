using Gambler.Bot.Core.Sites;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class AutomationRuntimeServiceTests
{
    [Fact]
    public void StartWithoutSelectedSiteFails()
    {
        var fixture = RuntimeFixture.Create();

        var result = fixture.Runtime.Start();

        Assert.False(result.Succeeded);
        Assert.Equal("Select or simulate a site before starting.", result.Message);
        Assert.Equal("Idle", fixture.Automation.Current.Status);
    }

    [Fact]
    public void StartWithDisconnectedSiteFails()
    {
        var fixture = RuntimeFixture.Create();
        fixture.Sites.Select(TestData.Site);
        fixture.Strategies.Select(TestData.Strategy);

        var result = fixture.Runtime.Start();

        Assert.False(result.Succeeded);
        Assert.Equal("Use simulation mode or complete login before starting.", result.Message);
        Assert.Equal("Idle", fixture.Automation.Current.Status);
    }

    [Fact]
    public void StartWithoutSelectedStrategyFails()
    {
        var fixture = RuntimeFixture.Create();
        fixture.Sites.StartSimulation(TestData.Site);

        var result = fixture.Runtime.Start();

        Assert.False(result.Succeeded);
        Assert.Equal("Select a strategy before starting.", result.Message);
        Assert.Equal("Idle", fixture.Automation.Current.Status);
    }

    [Fact]
    public void StartWhenSiteCannotBeCreatedFails()
    {
        var fixture = RuntimeFixture.Create();
        fixture.Sites.StartSimulation(TestData.Site);
        fixture.Strategies.Select(TestData.Strategy);

        var result = fixture.Runtime.Start();

        Assert.False(result.Succeeded);
        Assert.Equal("Test Site could not be created from Core.", result.Message);
        Assert.Equal("Idle", fixture.Automation.Current.Status);
    }

    [Fact]
    public void PauseAndStopUpdateRuntimeState()
    {
        var fixture = RuntimeFixture.Create();

        var pause = fixture.Runtime.Pause();
        var stop = fixture.Runtime.Stop();

        Assert.True(pause.Succeeded);
        Assert.True(stop.Succeeded);
        Assert.Equal("Idle", fixture.Automation.Current.Status);
        Assert.Null(fixture.Automation.Current.StartedAt);
    }

    private sealed record RuntimeFixture(
        SiteSessionService Sites,
        StrategySessionService Strategies,
        AutomationStateService Automation,
        AutomationRuntimeService Runtime)
    {
        public static RuntimeFixture Create()
        {
            var sites = new SiteSessionService();
            var strategies = new StrategySessionService();
            var automation = new AutomationStateService();
            var runtime = new AutomationRuntimeService(
                sites,
                strategies,
                automation,
                new EmptySiteCatalogService(),
                new EmptyStrategyCatalogService());

            return new RuntimeFixture(sites, strategies, automation, runtime);
        }
    }

    private static class TestData
    {
        public static readonly SiteSummary Site = new(
            "Test Site",
            "https://example.invalid",
            ["BTC"],
            ["Dice"],
            "Test",
            "Missing.Site");

        public static readonly StrategySummary Strategy = new(
            "Test Strategy",
            "Built-in",
            "Strategy",
            "Test",
            "Missing.Strategy");
    }

    private sealed class EmptySiteCatalogService : ISiteCatalogService
    {
        public IReadOnlyList<SiteSummary> GetSites()
        {
            return [];
        }

        public BaseSite? CreateSite(SiteSummary summary)
        {
            return null;
        }
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
