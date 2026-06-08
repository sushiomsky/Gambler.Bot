using Gambler.Bot.Core.Sites;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class InsightServiceTests
{
    [Fact]
    public async Task ReportsWarningsWhenNoSiteOrStrategyIsActive()
    {
        var fixture = InsightFixture.Create();

        var insights = await fixture.Service.GetInsightsAsync();

        Assert.Contains(insights, insight => insight.Title == "No active site" && insight.Severity == "Warning");
        Assert.Contains(insights, insight => insight.Title == "No active strategy" && insight.Severity == "Warning");
        Assert.Contains(insights, insight => insight.Title == "Risk guard" && insight.Severity == "Success");
    }

    [Fact]
    public async Task ReportsActiveSiteStrategyAndRecentHistory()
    {
        var fixture = InsightFixture.Create(
            history: [new BetHistoryRecord(DateTimeOffset.UtcNow, "Test Site", "Dice", "BTC", 1m, 0.5m, "Win")]);
        fixture.Sites.StartSimulation(TestData.Site);
        fixture.Strategies.Select(TestData.Strategy);

        var insights = await fixture.Service.GetInsightsAsync();

        Assert.Contains(insights, insight => insight.Title == "Active site" && insight.Severity == "Success");
        Assert.Contains(insights, insight => insight.Title == "Active strategy" && insight.Severity == "Success");
        Assert.Contains(insights, insight => insight.Title == "Recent bets" && insight.Severity == "Success");
    }

    private sealed record InsightFixture(
        SiteSessionService Sites,
        StrategySessionService Strategies,
        InsightService Service)
    {
        public static InsightFixture Create(IReadOnlyList<BetHistoryRecord>? history = null)
        {
            var sites = new SiteSessionService();
            var strategies = new StrategySessionService();
            var service = new InsightService(
                new StaticSettingsService(),
                sites,
                new StaticSiteCatalogService(),
                new StaticStrategyCatalogService(),
                strategies,
                new StaticBetHistoryService(history ?? []),
                new AutomationStateService());

            return new InsightFixture(sites, strategies, service);
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
            "Test.Site");

        public static readonly StrategySummary Strategy = new(
            "Test Strategy",
            "Built-in",
            "Strategy",
            "Test",
            "Test.Strategy");
    }

    private sealed class StaticSettingsService : IAppSettingsService
    {
        public Task<NativeUiSettings> LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new NativeUiSettings());
        }

        public Task SaveAsync(NativeUiSettings settings, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class StaticSiteCatalogService : ISiteCatalogService
    {
        public IReadOnlyList<SiteSummary> GetSites()
        {
            return [TestData.Site];
        }

        public BaseSite? CreateSite(SiteSummary summary)
        {
            return null;
        }
    }

    private sealed class StaticStrategyCatalogService : IStrategyCatalogService
    {
        public IReadOnlyList<StrategySummary> GetStrategies()
        {
            return [TestData.Strategy];
        }

        public BaseStrategy? CreateStrategy(StrategySummary summary)
        {
            return null;
        }
    }

    private sealed class StaticBetHistoryService : IBetHistoryService
    {
        private readonly IReadOnlyList<BetHistoryRecord> _history;

        public StaticBetHistoryService(IReadOnlyList<BetHistoryRecord> history)
        {
            _history = history;
        }

        public IReadOnlyList<BetHistoryRecord> GetRecent(string? siteName = null, int limit = 100)
        {
            return _history;
        }
    }
}
