using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class BetHistoryFilterServiceTests
{
    [Fact]
    public void EmptyFiltersReturnAllRecords()
    {
        var service = new BetHistoryFilterService();

        var records = service.Apply(TestData.Records, "", "All");

        Assert.Equal(3, records.Count);
    }

    [Fact]
    public void OutcomeFilterMatchesCaseInsensitive()
    {
        var service = new BetHistoryFilterService();

        var records = service.Apply(TestData.Records, null, "win");

        Assert.Equal(2, records.Count);
        Assert.All(records, record => Assert.Equal("Win", record.Outcome));
    }

    [Fact]
    public void SearchMatchesSiteGameCurrencyAndOutcome()
    {
        var service = new BetHistoryFilterService();

        Assert.Single(service.Apply(TestData.Records, "stake", "All"));
        Assert.Single(service.Apply(TestData.Records, "limbo", "All"));
        Assert.Single(service.Apply(TestData.Records, "eth", "All"));
        Assert.Single(service.Apply(TestData.Records, "loss", "All"));
    }

    [Fact]
    public void SearchMatchesVerifierFields()
    {
        var service = new BetHistoryFilterService();

        Assert.Single(service.Apply(TestData.Records, "server-alpha", "All"));
        Assert.Single(service.Apply(TestData.Records, "client-alpha", "All"));
        Assert.Single(service.Apply(TestData.Records, "42", "All"));
    }

    [Fact]
    public void CombinesSearchAndOutcomeFilters()
    {
        var service = new BetHistoryFilterService();

        var records = service.Apply(TestData.Records, "btc", "Win");

        var record = Assert.Single(records);
        Assert.Equal("Stake", record.Site);
    }

    [Fact]
    public void CriteriaFiltersByCurrency()
    {
        var service = new BetHistoryFilterService();

        var records = service.Apply(TestData.Records, new BetHistoryFilterCriteria(Currency: "eth"));

        var record = Assert.Single(records);
        Assert.Equal("Primedice", record.Site);
    }

    [Fact]
    public void CriteriaFiltersByProfitRange()
    {
        var service = new BetHistoryFilterService();

        var records = service.Apply(TestData.Records, new BetHistoryFilterCriteria(MinimumProfit: 0m, MaximumProfit: 0.75m));

        var record = Assert.Single(records);
        Assert.Equal("Stake", record.Site);
    }

    [Fact]
    public void CriteriaFiltersVerifierReadyRecords()
    {
        var service = new BetHistoryFilterService();

        var records = service.Apply(TestData.Records, new BetHistoryFilterCriteria(VerifierReadyOnly: true));

        var record = Assert.Single(records);
        Assert.True(record.CanPrefillVerifier);
    }

    [Fact]
    public void CriteriaCanCombineAdvancedFilters()
    {
        var service = new BetHistoryFilterService();
        var criteria = new BetHistoryFilterCriteria(
            SearchText: "dice",
            Outcome: "Win",
            Currency: "BTC",
            MinimumProfit: 0m,
            MaximumProfit: 1m,
            VerifierReadyOnly: true);

        var records = service.Apply(TestData.Records, criteria);

        var record = Assert.Single(records);
        Assert.Equal("Stake", record.Site);
    }

    private static class TestData
    {
        public static readonly IReadOnlyList<BetHistoryRecord> Records =
        [
            new(DateTimeOffset.UtcNow, "Stake", "Dice", "BTC", 1m, 0.5m, "Win", "server-alpha", "client-alpha", 42),
            new(DateTimeOffset.UtcNow, "Primedice", "Limbo", "ETH", 2m, -1m, "Loss"),
            new(DateTimeOffset.UtcNow, "DuckDice", "Twist", "DOGE", 3m, 1m, "Win")
        ];
    }
}
