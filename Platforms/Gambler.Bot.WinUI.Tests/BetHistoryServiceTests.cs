using System.Globalization;
using Gambler.Bot.WinUI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class BetHistoryServiceTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"gambler-bot-history-{Guid.NewGuid():N}.db");

    [Fact]
    public void MissingDatabaseReturnsEmptyHistory()
    {
        var service = new BetHistoryService(NullLogger<BetHistoryService>.Instance, _databasePath);

        var records = service.GetRecent();

        Assert.Empty(records);
    }

    [Fact]
    public void ReadsRecordsFromAvailableTables()
    {
        CreateDatabase();
        InsertDiceBet("Alpha", 1_700_000_000m, "BTC", "0.001", "0.0005", true);
        var service = new BetHistoryService(NullLogger<BetHistoryService>.Instance, _databasePath);

        var records = service.GetRecent();

        var record = Assert.Single(records);
        Assert.Equal("Alpha", record.Site);
        Assert.Equal("Dice", record.Game);
        Assert.Equal("BTC", record.Currency);
        Assert.Equal(0.001m, record.Amount);
        Assert.Equal(0.0005m, record.Profit);
        Assert.Equal("Win", record.Outcome);
    }

    [Fact]
    public void ReadsOptionalVerifierFieldsWhenAvailable()
    {
        CreateDatabase(includeVerifierFields: true);
        InsertDiceBet(
            "Alpha",
            1_700_000_000m,
            "BTC",
            "0.001",
            "0.0005",
            true,
            "server-seed",
            "client-seed",
            42);
        var service = new BetHistoryService(NullLogger<BetHistoryService>.Instance, _databasePath);

        var record = Assert.Single(service.GetRecent());

        Assert.Equal("server-seed", record.ServerSeed);
        Assert.Equal("client-seed", record.ClientSeed);
        Assert.Equal(42, record.Nonce);
        Assert.True(record.CanPrefillVerifier);
    }

    [Fact]
    public void FiltersRecordsBySite()
    {
        CreateDatabase();
        InsertDiceBet("Alpha", 1_700_000_000m, "BTC", "0.001", "0.0005", true);
        InsertDiceBet("Beta", 1_700_000_001m, "ETH", "0.002", "-0.002", false);
        var service = new BetHistoryService(NullLogger<BetHistoryService>.Instance, _databasePath);

        var records = service.GetRecent("Beta");

        var record = Assert.Single(records);
        Assert.Equal("Beta", record.Site);
        Assert.Equal("Loss", record.Outcome);
    }

    [Fact]
    public void AppliesLimitAfterSortingNewestFirst()
    {
        CreateDatabase();
        InsertDiceBet("Alpha", 1_700_000_000m, "BTC", "0.001", "0.0005", true);
        InsertDiceBet("Alpha", 1_700_000_010m, "BTC", "0.002", "0.001", true);
        var service = new BetHistoryService(NullLogger<BetHistoryService>.Instance, _databasePath);

        var records = service.GetRecent(limit: 1);

        var record = Assert.Single(records);
        Assert.Equal(0.002m, record.Amount);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }

    private void CreateDatabase(bool includeVerifierFields = false)
    {
        using var connection = new SqliteConnection($"Data Source={_databasePath};Pooling=False");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = includeVerifierFields
            ? """
                create table DiceBets (
                    Date text not null,
                    Site text not null,
                    Game integer not null,
                    Currency text not null,
                    TotalAmount text not null,
                    Profit text not null,
                    IsWin integer not null,
                    ServerSeed text null,
                    ClientSeed text null,
                    Nonce integer null
                );
                """
            : """
                create table DiceBets (
                    Date text not null,
                    Site text not null,
                    Game integer not null,
                    Currency text not null,
                    TotalAmount text not null,
                    Profit text not null,
                    IsWin integer not null
                );
                """;
        command.ExecuteNonQuery();
    }

    private void InsertDiceBet(
        string site,
        decimal epochSeconds,
        string currency,
        string amount,
        string profit,
        bool isWin,
        string? serverSeed = null,
        string? clientSeed = null,
        long? nonce = null)
    {
        using var connection = new SqliteConnection($"Data Source={_databasePath};Pooling=False");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = serverSeed is null && clientSeed is null && nonce is null
            ? """
                insert into DiceBets (Date, Site, Game, Currency, TotalAmount, Profit, IsWin)
                values ($date, $site, 0, $currency, $amount, $profit, $isWin);
                """
            : """
                insert into DiceBets (Date, Site, Game, Currency, TotalAmount, Profit, IsWin, ServerSeed, ClientSeed, Nonce)
                values ($date, $site, 0, $currency, $amount, $profit, $isWin, $serverSeed, $clientSeed, $nonce);
                """;
        command.Parameters.AddWithValue("$date", epochSeconds.ToString(CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$site", site);
        command.Parameters.AddWithValue("$currency", currency);
        command.Parameters.AddWithValue("$amount", amount);
        command.Parameters.AddWithValue("$profit", profit);
        command.Parameters.AddWithValue("$isWin", isWin ? 1 : 0);
        if (serverSeed is not null || clientSeed is not null || nonce is not null)
        {
            command.Parameters.AddWithValue("$serverSeed", serverSeed ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$clientSeed", clientSeed ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$nonce", (object?)nonce ?? DBNull.Value);
        }

        command.ExecuteNonQuery();
    }
}
