using Gambler.Bot.Common.Games;
using Gambler.Bot.Common.Helpers;
using Gambler.Bot.Common.Interfaces;
using Gambler.Bot.Core.Helpers;
using Gambler.Bot.Core.Sites;
using Gambler.Bot.Core.Sites.Classes;
using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class LiveLoginServiceTests
{
    [Fact]
    public async Task LoginAsyncTriesNextMirrorWhenFirstFails()
    {
        var catalog = new FallbackSiteCatalogService("https://good.example");
        var session = new SiteSessionService();
        var service = new LiveLoginService(
            catalog,
            session,
            new StaticSettingsService(),
            NullLogger<LiveLoginService>.Instance);

        var profile = CreateProfile(["https://bad.example", "https://good.example"]);

        var result = await service.LoginAsync(profile);

        Assert.True(result.Succeeded);
        Assert.Contains("https://good.example", result.Message);
        Assert.Equal("Live", session.Current.Mode);
        Assert.Equal(2, catalog.CreatedSites.Count);
        Assert.Equal("https://bad.example", catalog.CreatedSites[0].AttemptedUrl);
        Assert.Equal("https://good.example", catalog.CreatedSites[1].AttemptedUrl);
        Assert.Equal("DECOY", session.Current.RuntimeSite?.CurrentCurrency);
        Assert.Null(profile.Fields[0].Value);
    }

    [Fact]
    public async Task LoginAsyncUsesProfileCurrencyBeforeDefaultSettingsCurrency()
    {
        var catalog = new FallbackSiteCatalogService("https://good.example");
        var session = new SiteSessionService();
        var service = new LiveLoginService(
            catalog,
            session,
            new StaticSettingsService(defaultCurrency: "BTC"),
            NullLogger<LiveLoginService>.Instance);
        var profile = CreateProfile(["https://good.example"], ["DECOY", "BTC"]);
        profile.SelectedCurrency = "DECOY";

        var result = await service.LoginAsync(profile);

        Assert.True(result.Succeeded);
        Assert.Contains("using DECOY", result.Message);
        Assert.Equal("DECOY", session.Current.RuntimeSite?.CurrentCurrency);
    }

    [Fact]
    public async Task LoginAsyncReportsAllMirrorsWhenEveryAttemptFails()
    {
        var catalog = new FallbackSiteCatalogService("https://missing.example");
        var service = new LiveLoginService(
            catalog,
            new SiteSessionService(),
            new StaticSettingsService(),
            NullLogger<LiveLoginService>.Instance);

        var profile = CreateProfile(["https://bad-one.example", "https://bad-two.example"]);

        var result = await service.LoginAsync(profile);

        Assert.False(result.Succeeded);
        Assert.Contains("bad-one", result.Message);
        Assert.Contains("bad-two", result.Message);
        Assert.Null(profile.Fields[0].Value);
    }

    private static LoginProfile CreateProfile(IReadOnlyList<string> mirrors, IReadOnlyList<string>? currencies = null)
    {
        return new LoginProfile(
            TestData.Site,
            SupportsNormalLogin: true,
            SupportsBrowserLogin: false,
            mirrors,
            currencies ?? ["DECOY"],
            [
                new LoginFieldModel
                {
                    Name = "API Key",
                    IsRequired = true,
                    IsSecret = true,
                    Value = "test-key"
                }
            ]);
    }

    private static class TestData
    {
        public static readonly SiteSummary Site = new(
            "DuckDice Test",
            "https://duckdice.test",
            ["DECOY"],
            ["Dice"],
            "Available",
            typeof(FallbackSite).FullName!);
    }

    private sealed class FallbackSiteCatalogService(string successfulUrl) : ISiteCatalogService
    {
        public List<FallbackSite> CreatedSites { get; } = [];

        public IReadOnlyList<SiteSummary> GetSites()
        {
            return [TestData.Site];
        }

        public BaseSite? CreateSite(SiteSummary summary)
        {
            var site = new FallbackSite(successfulUrl);
            CreatedSites.Add(site);
            return site;
        }
    }

    private sealed class StaticSettingsService(string defaultCurrency = "DECOY") : IAppSettingsService
    {
        public Task<NativeUiSettings> LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new NativeUiSettings { DefaultCurrency = defaultCurrency });
        }

        public Task SaveAsync(NativeUiSettings settings, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FallbackSite : BaseSite
    {
        private readonly string _successfulUrl;

        public FallbackSite(string successfulUrl)
            : base(NullLogger.Instance)
        {
            _successfulUrl = successfulUrl;
            StaticLoginParams = [new LoginParameter("API Key", true, true, false, true)];
            SiteName = "DuckDice Test";
            SiteURL = "https://duckdice.test";
            Currencies = ["DECOY"];
            SupportedGames = [Games.Dice];
            Stats = new SiteStats();
        }

        public string? AttemptedUrl { get; private set; }

        public override void SetProxy(ProxyDetails ProxyInfo)
        {
        }

        protected override Task<bool> _Login(LoginParamValue[] LoginParams)
        {
            AttemptedUrl = URLInUse;
            return Task.FromResult(
                string.Equals(URLInUse, _successfulUrl, StringComparison.OrdinalIgnoreCase)
                && LoginParams.FirstOrDefault()?.Value == "test-key");
        }

        protected override Task<bool> _BrowserLogin()
        {
            return Task.FromResult(false);
        }

        protected override void _Disconnect()
        {
        }

        protected override Task<SiteStats> _UpdateStats()
        {
            return Task.FromResult(Stats);
        }

        protected override IGameResult _GetLucky(string ServerSeed, string ClientSeed, int Nonce, Games Game)
        {
            return null!;
        }
    }
}
