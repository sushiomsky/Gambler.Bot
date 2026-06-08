using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class BetHistorySummaryServiceTests
{
    [Fact]
    public void EmptyHistoryReturnsZeroSummary()
    {
        var service = new BetHistorySummaryService();

        var summary = service.Summarize([]);

        Assert.Equal(0, summary.TotalRecords);
        Assert.Equal(0, summary.Wins);
        Assert.Equal(0, summary.Losses);
        Assert.Equal(0m, summary.TotalAmount);
        Assert.Equal(0m, summary.NetProfit);
        Assert.Equal(0m, summary.WinRate);
    }

    [Fact]
    public void SummarizesRecords()
    {
        var service = new BetHistorySummaryService();

        var summary = service.Summarize(
        [
            new BetHistoryRecord(DateTimeOffset.UtcNow, "Stake", "Dice", "BTC", 1m, 0.5m, "Win"),
            new BetHistoryRecord(DateTimeOffset.UtcNow, "Stake", "Dice", "BTC", 2m, -2m, "Loss"),
            new BetHistoryRecord(DateTimeOffset.UtcNow, "Stake", "Dice", "BTC", 3m, 1m, "Win")
        ]);

        Assert.Equal(3, summary.TotalRecords);
        Assert.Equal(2, summary.Wins);
        Assert.Equal(1, summary.Losses);
        Assert.Equal(6m, summary.TotalAmount);
        Assert.Equal(-0.5m, summary.NetProfit);
        Assert.Equal(66.67m, summary.WinRate);
    }
}
