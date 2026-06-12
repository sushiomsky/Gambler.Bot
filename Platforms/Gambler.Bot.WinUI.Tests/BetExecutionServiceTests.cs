using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Sites.Classes;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class BetExecutionServiceTests
{
    [Fact]
    public async Task ExecuteLiveBetRequiresLiveSession()
    {
        var fixture = LiveBetFixture.Create();

        var result = await fixture.Service.ExecuteLiveBetAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Complete live login before placing a live bet.", result.Message);
    }

    [Fact]
    public async Task ExecuteLiveBetRequiresSafetyGate()
    {
        var fixture = LiveBetFixture.Create();
        fixture.ConnectLive(currency: "DECOY");
        fixture.Strategies.Select(TestData.Strategy);

        var result = await fixture.Service.ExecuteLiveBetAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Live bet execution is locked. Enable it in settings and enter the exact confirmation phrase.", result.Message);
    }

    [Fact]
    public async Task ExecuteLiveBetRequiresDecoyWhenConfigured()
    {
        var fixture = LiveBetFixture.Create(settings: LiveSettings());
        fixture.ConnectLive(currency: "BTC");
        fixture.Strategies.Select(TestData.Strategy);

        var result = await fixture.Service.ExecuteLiveBetAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Live bet execution requires DECOY currency in settings.", result.Message);
    }

    [Fact]
    public async Task ExecuteLiveBetRejectsAmountAboveMaximum()
    {
        var fixture = LiveBetFixture.Create(settings: LiveSettings(maximumLiveBet: 0.01m), strategyAmount: 0.02m);
        fixture.ConnectLive(currency: "DECOY");
        fixture.Strategies.Select(TestData.Strategy);

        var result = await fixture.Service.ExecuteLiveBetAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Live bet amount 0.02 exceeds maximum 0.01.", result.Message);
        Assert.Equal(0, fixture.Site.PlacedBets);
    }

    [Fact]
    public async Task ExecuteLiveBetPlacesSingleGuardedLiveBet()
    {
        var fixture = LiveBetFixture.Create(settings: LiveSettings(maximumLiveBetsPerRun: 1), strategyAmount: 0.001m);
        fixture.ConnectLive(currency: "DECOY");
        fixture.Strategies.Select(TestData.Strategy);

        var first = await fixture.Service.ExecuteLiveBetAsync();
        var second = await fixture.Service.ExecuteLiveBetAsync();

        Assert.True(first.Succeeded);
        Assert.Contains("Live bet placed: 0.01 DECOY", first.Message);
        Assert.Equal(1, fixture.Site.PlacedBets);
        Assert.False(second.Succeeded);
        Assert.Equal("Live bet limit reached (1 per run).", second.Message);
    }

    private static NativeUiSettings LiveSettings(decimal maximumLiveBet = 0.01m, int maximumLiveBetsPerRun = 10)
    {
        return new NativeUiSettings
        {
            AllowLiveBetExecution = true,
            LiveBetConfirmationPhrase = "PLACE LIVE BETS",
            RequireDecoyCurrencyForLiveBets = true,
            MinimumBetAmount = 0.01m,
            MaximumLiveBetAmount = maximumLiveBet,
            MaximumLiveBetsPerRun = maximumLiveBetsPerRun
        };
    }

    private sealed record LiveBetFixture(
        SiteSessionService Sites,
        StrategySessionService Strategies,
        TestLiveSite Site,
        BetExecutionService Service)
    {
        public static LiveBetFixture Create(NativeUiSettings? settings = null, decimal strategyAmount = 0.01m)
        {
            var sites = new SiteSessionService();
            var strategies = new StrategySessionService();
            var site = new TestLiveSite();
            var service = new BetExecutionService(
                sites,
                strategies,
                new StaticSiteCatalogService(site),
                new StaticStrategyCatalogService(strategyAmount),
                new StaticSettingsService(settings ?? new NativeUiSettings()));

            return new LiveBetFixture(sites, strategies, site, service);
        }

        public void ConnectLive(string currency)
        {
            Site.CurrentCurrency = currency;
            Site.LoggedIn = true;
            Sites.SetLiveConnected(TestData.Site, Site);
        }
    }

    private static class TestData
    {
        public static readonly SiteSummary Site = new(
            "Live Test Site",
            "https://example.invalid",
            ["DECOY", "BTC"],
            ["Dice"],
            "Test",
            "Test.LiveSite");

        public static readonly StrategySummary Strategy = new(
            "Live Test Strategy",
            "Preset",
            "Native",
            "Test",
            "Test.LiveStrategy");
    }

    private sealed class StaticSettingsService(NativeUiSettings settings) : IAppSettingsService
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

    private sealed class StaticSiteCatalogService(TestLiveSite site) : ISiteCatalogService
    {
        public IReadOnlyList<SiteSummary> GetSites()
        {
            return [TestData.Site];
        }

        public BaseSite? CreateSite(SiteSummary summary)
        {
            return site;
        }
    }

    private sealed class StaticStrategyCatalogService(decimal amount) : IStrategyCatalogService
    {
        public IReadOnlyList<StrategySummary> GetStrategies()
        {
            return [TestData.Strategy];
        }

        public BaseStrategy? CreateStrategy(StrategySummary summary)
        {
            return new TestStrategy(amount);
        }
    }

    private sealed class TestStrategy(decimal amount) : BaseStrategy
    {
        public override string StrategyName { get; protected set; } = "Live Test Strategy";

        protected override PlaceBet NextBet(Bet PreviousBet, bool Win)
        {
            return RunReset(Games.Dice);
        }

        public override PlaceBet RunReset(Games game)
        {
            return new PlaceDiceBet(amount, true, 49.5m);
        }
    }

    private sealed class TestLiveSite : BaseSite, iDice
    {
        public int PlacedBets { get; private set; }
        public DiceConfig DiceSettings { get; set; } = new() { Edge = 1, MaxRoll = 99.99m };

        public TestLiveSite()
        {
            SiteName = "Live Test Site";
            SupportedGames = [Games.Dice];
            CurrentCurrency = "DECOY";
        }

        public Task<DiceBet> PlaceDiceBet(PlaceDiceBet BetDetails)
        {
            PlacedBets++;
            return Task.FromResult(new DiceBet
            {
                BetID = $"bet-{PlacedBets}",
                TotalAmount = BetDetails.Amount,
                Currency = CurrentCurrency,
                Profit = 0.01m,
                Chance = BetDetails.Chance,
                High = BetDetails.High,
                Roll = 70m
            });
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
            return new DiceResult { Roll = 70m };
        }
    }
}
