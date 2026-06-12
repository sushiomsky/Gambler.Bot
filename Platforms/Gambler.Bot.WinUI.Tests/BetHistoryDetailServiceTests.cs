using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class BetHistoryDetailServiceTests
{
    [Fact]
    public void CreateDetailsIncludesCoreBetFields()
    {
        var service = new BetHistoryDetailService();
        var record = CreateRecord();

        var details = service.CreateDetails(record);

        Assert.Contains(details, item => item.Label == "Site" && item.Value == "DuckDice");
        Assert.Contains(details, item => item.Label == "Game" && item.Value == "Dice");
        Assert.Contains(details, item => item.Label == "Amount" && item.Value == "0.01");
        Assert.Contains(details, item => item.Label == "Profit" && item.Value == "0.02");
        Assert.Contains(details, item => item.Label == "Outcome" && item.Value == "Win");
    }

    [Fact]
    public void CreateDetailsIncludesVerifierFieldsWhenPresent()
    {
        var service = new BetHistoryDetailService();
        var record = CreateRecord("server", "client", 42);

        var details = service.CreateDetails(record);

        Assert.Contains(details, item => item.Label == "Verifier status" && item.Value == "Fair");
        Assert.Contains(details, item => item.Label == "Server seed" && item.Value == "server");
        Assert.Contains(details, item => item.Label == "Client seed" && item.Value == "client");
        Assert.Contains(details, item => item.Label == "Nonce" && item.Value == "42");
    }

    [Fact]
    public void CreateDetailsUsesDashForMissingVerifierFields()
    {
        var service = new BetHistoryDetailService();
        var record = CreateRecord();

        var details = service.CreateDetails(record);

        Assert.Contains(details, item => item.Label == "Verifier status" && item.Value == "No seed");
        Assert.Contains(details, item => item.Label == "Server seed" && item.Value == "-");
        Assert.Contains(details, item => item.Label == "Client seed" && item.Value == "-");
        Assert.Contains(details, item => item.Label == "Nonce" && item.Value == "-");
    }

    private static BetHistoryRecord CreateRecord(string? serverSeed = null, string? clientSeed = null, long? nonce = null)
    {
        return new BetHistoryRecord(
            DateTimeOffset.Parse("2026-06-08T10:15:30Z"),
            "DuckDice",
            "Dice",
            "DECOY",
            0.01m,
            0.02m,
            "Win",
            serverSeed,
            clientSeed,
            nonce);
    }
}
