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
    public void CombinesSearchAndOutcomeFilters()
    {
        var service = new BetHistoryFilterService();

        var records = service.Apply(TestData.Records, "btc", "Win");

        var record = Assert.Single(records);
        Assert.Equal("Stake", record.Site);
    }

    private static class TestData
    {
        public static readonly IReadOnlyList<BetHistoryRecord> Records =
        [
            new(DateTimeOffset.UtcNow, "Stake", "Dice", "BTC", 1m, 0.5m, "Win"),
            new(DateTimeOffset.UtcNow, "Primedice", "Limbo", "ETH", 2m, -1m, "Loss"),
            new(DateTimeOffset.UtcNow, "DuckDice", "Twist", "DOGE", 3m, 1m, "Win")
        ];
    }
}
