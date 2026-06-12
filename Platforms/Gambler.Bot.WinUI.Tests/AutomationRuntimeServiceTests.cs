using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Sites.Classes;
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

    [Fact]
    public async Task StartRunsSimulationLoopUntilConfiguredLimit()
    {
        var fixture = RuntimeFixture.Create(
            siteCatalog: new WorkingSiteCatalogService(),
            strategyCatalog: new WorkingStrategyCatalogService(),
            betExecution: new CountingBetExecutionService(),
            settings: new NativeUiSettings
            {
                AutomationLoopDelayMs = 100,
                AutomationMaxSimulationIterations = 3
            });
        fixture.Sites.StartSimulation(TestData.Site);
        fixture.Strategies.Select(TestData.Strategy);

        var result = fixture.Runtime.Start();

        Assert.True(result.Succeeded);
        await WaitUntilAsync(() => fixture.Automation.Current.Status == "Completed");
        Assert.Equal(3, fixture.Automation.Current.LoopIterations);
        Assert.Equal("Simulation iteration limit reached.", fixture.Automation.Current.LastMessage);
    }

    [Fact]
    public void StartWhileLoopIsRunningFails()
    {
        var fixture = RuntimeFixture.Create(
            siteCatalog: new WorkingSiteCatalogService(),
            strategyCatalog: new WorkingStrategyCatalogService(),
            betExecution: new CountingBetExecutionService(),
            settings: new NativeUiSettings
            {
                AutomationLoopDelayMs = 1000,
                AutomationMaxSimulationIterations = 0
            });
        fixture.Sites.StartSimulation(TestData.Site);
        fixture.Strategies.Select(TestData.Strategy);

        var first = fixture.Runtime.Start();
        var second = fixture.Runtime.Start();
        fixture.Runtime.Stop();

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Equal("Automation loop is already running.", second.Message);
    }

    [Fact]
    public void LiveStartRequiresExplicitSafetyGate()
    {
        var fixture = RuntimeFixture.Create(
            siteCatalog: new WorkingSiteCatalogService(),
            strategyCatalog: new WorkingStrategyCatalogService(),
            betExecution: new CountingBetExecutionService());
        fixture.Sites.SetLiveConnected(TestData.Site);
        fixture.Strategies.Select(TestData.Strategy);

        var result = fixture.Runtime.Start();

        Assert.False(result.Succeeded);
        Assert.Equal("Live bet execution is locked. Enable it in settings and enter the exact confirmation phrase.", result.Message);
        Assert.Equal("Idle", fixture.Automation.Current.Status);
    }

    private sealed record RuntimeFixture(
        SiteSessionService Sites,
        StrategySessionService Strategies,
        AutomationStateService Automation,
        AutomationRuntimeService Runtime)
    {
        public static RuntimeFixture Create(
            ISiteCatalogService? siteCatalog = null,
            IStrategyCatalogService? strategyCatalog = null,
            IBetExecutionService? betExecution = null,
            NativeUiSettings? settings = null)
        {
            var sites = new SiteSessionService();
            var strategies = new StrategySessionService();
            var automation = new AutomationStateService();
            var runtime = new AutomationRuntimeService(
                sites,
                strategies,
                automation,
                siteCatalog ?? new EmptySiteCatalogService(),
                strategyCatalog ?? new EmptyStrategyCatalogService(),
                betExecution ?? new CountingBetExecutionService(),
                new InMemorySettingsService(settings ?? new NativeUiSettings()));

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

    private sealed class WorkingSiteCatalogService : ISiteCatalogService
    {
        public IReadOnlyList<SiteSummary> GetSites()
        {
            return [TestData.Site];
        }

        public BaseSite? CreateSite(SiteSummary summary)
        {
            return new TestSite();
        }
    }

    private sealed class WorkingStrategyCatalogService : IStrategyCatalogService
    {
        public IReadOnlyList<StrategySummary> GetStrategies()
        {
            return [TestData.Strategy];
        }

        public BaseStrategy? CreateStrategy(StrategySummary summary)
        {
            return new TestStrategy();
        }
    }

    private sealed class CountingBetExecutionService : IBetExecutionService
    {
        public int Calls { get; private set; }

        public AutomationCommandResult PrepareNextBet(out PlaceBetPreview? preview)
        {
            Calls++;
            preview = new PlaceBetPreview("Test Site", "Test Strategy", "Dice", "BTC", 0.00000001m, $"Iteration {Calls}");
            return new AutomationCommandResult(true, $"Prepared iteration {Calls}.");
        }

        public Task<AutomationCommandResult> ExecuteLiveBetAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AutomationCommandResult(false, "Live execution is not used by this fixture."));
        }
    }

    private sealed class InMemorySettingsService(NativeUiSettings settings) : IAppSettingsService
    {
        public Task<NativeUiSettings> LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(settings);
        }

        public Task SaveAsync(NativeUiSettings settings, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestSite : BaseSite
    {
        public TestSite()
        {
            SiteName = "Test Site";
            CurrentCurrency = "BTC";
            SupportedGames = [Games.Dice];
        }

        protected override Task<bool> _Login(LoginParamValue[] LoginParams)
        {
            return Task.FromResult(true);
        }

        protected override Task<bool> _BrowserLogin()
        {
            return Task.FromResult(true);
        }

        protected override void _Disconnect()
        {
        }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
        }

        protected override Task<SiteStats> _UpdateStats()
        {
            return Task.FromResult(new SiteStats());
        }

        protected override IGameResult _GetLucky(string ServerSeed, string ClientSeed, int Nonce, Games Game)
        {
            return new TestGameResult(Game);
        }
    }

    private sealed class TestStrategy : BaseStrategy
    {
        public override string StrategyName { get; protected set; } = "Test Strategy";

        protected override PlaceBet NextBet(Bet PreviousBet, bool Win)
        {
            return RunReset(Games.Dice);
        }

        public override PlaceBet RunReset(Games game)
        {
            return CreateEmptyPlaceBet(game);
        }
    }

    private sealed record TestGameResult(Games Game) : IGameResult;

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!condition())
        {
            cancellation.Token.ThrowIfCancellationRequested();
            await Task.Delay(25, cancellation.Token);
        }
    }
}
