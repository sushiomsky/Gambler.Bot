using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class BetHistoryExportServiceTests : IDisposable
{
    private readonly string _filePath = Path.Combine(Path.GetTempPath(), $"gambler-bot-export-{Guid.NewGuid():N}.csv");

    [Fact]
    public async Task WritesCsvHeaderForEmptyHistory()
    {
        var service = new BetHistoryExportService();

        await service.ExportCsvAsync([], _filePath);

        var lines = await File.ReadAllLinesAsync(_filePath);
        var line = Assert.Single(lines);
        Assert.Equal("Timestamp,Site,Game,Currency,Amount,Profit,Outcome,ServerSeed,ClientSeed,Nonce", line);
    }

    [Fact]
    public async Task WritesRecordsWithInvariantAmounts()
    {
        var service = new BetHistoryExportService();
        var records = new[]
        {
            new BetHistoryRecord(
                DateTimeOffset.Parse("2026-06-08T10:15:30Z"),
                "Stake",
                "Dice",
                "BTC",
                0.001m,
                -0.0005m,
                "Loss",
                "server",
                "client",
                7)
        };

        await service.ExportCsvAsync(records, _filePath);

        var lines = await File.ReadAllLinesAsync(_filePath);
        Assert.Equal("2026-06-08T10:15:30.0000000+00:00,Stake,Dice,BTC,0.001,-0.0005,Loss,server,client,7", lines[1]);
    }

    [Fact]
    public async Task EscapesCsvValues()
    {
        var service = new BetHistoryExportService();
        var records = new[]
        {
            new BetHistoryRecord(
                DateTimeOffset.Parse("2026-06-08T10:15:30Z"),
                "Site, With Comma",
                "Dice",
                "BT\"C",
                1m,
                2m,
                "Win")
        };

        await service.ExportCsvAsync(records, _filePath);

        var lines = await File.ReadAllLinesAsync(_filePath);
        Assert.Contains("\"Site, With Comma\"", lines[1]);
        Assert.Contains("\"BT\"\"C\"", lines[1]);
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }
}
