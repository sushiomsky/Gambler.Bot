using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Sites.Classes;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class SessionServiceTests
{
    [Fact]
    public void SiteSelectionSetsDisconnectedSelectedMode()
    {
        var service = new SiteSessionService();

        service.Select(TestData.Site);

        Assert.Equal(TestData.Site, service.Current.SelectedSite);
        Assert.Equal("Selected", service.Current.Mode);
        Assert.False(service.Current.IsConnected);
    }

    [Fact]
    public void SiteSimulationSetsConnectedSimulationMode()
    {
        var service = new SiteSessionService();

        service.StartSimulation(TestData.Site);

        Assert.Equal(TestData.Site, service.Current.SelectedSite);
        Assert.Equal("Simulation", service.Current.Mode);
        Assert.True(service.Current.IsConnected);
    }

    [Fact]
    public void LiveConnectionSetsConnectedLiveMode()
    {
        var service = new SiteSessionService();
        var runtimeSite = new TestSite();

        service.SetLiveConnected(TestData.Site, runtimeSite);

        Assert.Equal(TestData.Site, service.Current.SelectedSite);
        Assert.Equal("Live", service.Current.Mode);
        Assert.True(service.Current.IsConnected);
        Assert.Equal(runtimeSite, service.Current.RuntimeSite);
    }

    [Fact]
    public void SiteClearReturnsToEmptyState()
    {
        var service = new SiteSessionService();
        service.StartSimulation(TestData.Site);

        service.Clear();

        Assert.Null(service.Current.SelectedSite);
        Assert.Equal("Idle", service.Current.Mode);
        Assert.False(service.Current.IsConnected);
    }

    [Fact]
    public void StrategySelectionAndClearUpdateState()
    {
        var service = new StrategySessionService();

        service.Select(TestData.Strategy);
        Assert.Equal(TestData.Strategy, service.Current.SelectedStrategy);

        service.Clear();
        Assert.Null(service.Current.SelectedStrategy);
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

    private sealed class TestSite : BaseSite
    {
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
            return null!;
        }
    }
}
