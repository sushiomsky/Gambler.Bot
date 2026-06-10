using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Games.Dice;
using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Sites.Classes;
using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class RollVerifierServiceTests
{
    [Fact]
    public void VerifyRequiresServerSeed()
    {
        var service = new RollVerifierService(new TestSiteCatalogService());

        var result = service.Verify(CreateRequest(serverSeed: ""));

        Assert.False(result.Succeeded);
        Assert.Equal("Server seed is required.", result.Message);
    }

    [Fact]
    public void VerifyFailsWhenSiteCannotBeCreated()
    {
        var service = new RollVerifierService(new EmptySiteCatalogService());

        var result = service.Verify(CreateRequest());

        Assert.False(result.Succeeded);
        Assert.Equal("Verifier Site could not be created from Core.", result.Message);
    }

    [Fact]
    public void VerifyFailsWhenSiteDoesNotSupportVerification()
    {
        var service = new RollVerifierService(new TestSiteCatalogService(canVerify: false));

        var result = service.Verify(CreateRequest());

        Assert.False(result.Succeeded);
        Assert.Equal("Verifier Site does not advertise roll verification support.", result.Message);
    }

    [Fact]
    public void VerifyReturnsFormattedDiceResult()
    {
        var service = new RollVerifierService(new TestSiteCatalogService());

        var result = service.Verify(CreateRequest());

        Assert.True(result.Succeeded);
        Assert.Equal("Roll verified.", result.Message);
        Assert.Equal("Verifier Site", result.Site);
        Assert.Equal("Dice", result.Game);
        Assert.Equal("hash-server", result.ServerSeedHash);
        Assert.Equal("Dice roll", result.ResultType);
        Assert.Equal("42.42", result.ResultValue);
    }

    private static RollVerificationRequest CreateRequest(string serverSeed = "server")
    {
        return new RollVerificationRequest(
            TestData.Site,
            "Dice",
            serverSeed,
            "client",
            7);
    }

    private static class TestData
    {
        public static readonly SiteSummary Site = new(
            "Verifier Site",
            "https://example.invalid",
            ["DECOY"],
            ["Dice"],
            "Available",
            "Verifier.Site");
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

    private sealed class TestSiteCatalogService(bool canVerify = true) : ISiteCatalogService
    {
        public IReadOnlyList<SiteSummary> GetSites()
        {
            return [TestData.Site];
        }

        public BaseSite? CreateSite(SiteSummary summary)
        {
            return new TestSite(canVerify);
        }
    }

    private sealed class TestSite : BaseSite
    {
        public TestSite(bool canVerify)
        {
            SiteName = "Verifier Site";
            CanVerify = canVerify;
            SupportedGames = [Games.Dice];
        }

        public override string GetHash(string ServerSeed)
        {
            return $"hash-{ServerSeed}";
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
            return new DiceResult { Roll = 42.42m };
        }
    }
}
