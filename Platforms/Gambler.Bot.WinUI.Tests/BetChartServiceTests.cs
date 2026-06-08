using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class BetChartServiceTests
{
    [Fact]
    public void EmptyRecordsReturnEmptySnapshot()
    {
        var service = new BetChartService();

        var snapshot = service.CreateSnapshot([]);

        Assert.Equal("No data", snapshot.Sparkline);
        Assert.Equal(0, snapshot.EndProfit);
        Assert.Equal(0, snapshot.Wins);
        Assert.Equal(0, snapshot.Losses);
    }

    [Fact]
    public void SnapshotUsesCumulativeProfitAndOutcomeCounts()
    {
        var service = new BetChartService();
        var records = new[]
        {
            CreateRecord(3, -0.5m, "Loss"),
            CreateRecord(1, 1.0m, "Win"),
            CreateRecord(2, 0.25m, "Win")
        };

        var snapshot = service.CreateSnapshot(records);

        Assert.NotEqual("No data", snapshot.Sparkline);
        Assert.Equal(1.0m, snapshot.StartProfit);
        Assert.Equal(0.75m, snapshot.EndProfit);
        Assert.Equal(1.25m, snapshot.BestProfit);
        Assert.Equal(0.75m, snapshot.WorstProfit);
        Assert.Equal(2, snapshot.Wins);
        Assert.Equal(1, snapshot.Losses);
    }

    private static BetHistoryRecord CreateRecord(int minute, decimal profit, string outcome)
    {
        return new BetHistoryRecord(
            new DateTimeOffset(2026, 6, 8, 12, minute, 0, TimeSpan.Zero),
            "DuckDice",
            "Dice",
            "DECOY",
            0.01m,
            profit,
            outcome);
    }
}
