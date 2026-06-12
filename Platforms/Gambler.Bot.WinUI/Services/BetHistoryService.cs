using System.Globalization;
using Gambler.Bot.WinUI.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Gambler.Bot.WinUI.Services;

public sealed class BetHistoryService : IBetHistoryService
{
    private static readonly string[] BetTables =
    [
        "DiceBets",
        "LimboBets",
        "TwistBets",
        "CrashBets",
        "PlinkoBets",
        "RouletteBets"
    ];

    private readonly ILogger<BetHistoryService> _logger;
    private readonly string? _databasePath;

    public BetHistoryService(ILogger<BetHistoryService> logger, string? databasePath = null)
    {
        _logger = logger;
        _databasePath = databasePath;
    }

    public IReadOnlyList<BetHistoryRecord> GetRecent(string? siteName = null, int limit = 100)
    {
        var databasePath = _databasePath ?? FindDatabasePath();
        if (databasePath is null)
        {
            return [];
        }

        try
        {
            var records = new List<BetHistoryRecord>();
            using var connection = new SqliteConnection($"Data Source={databasePath};Pooling=False");
            connection.Open();

            foreach (var table in BetTables)
            {
                if (!TableExists(connection, table))
                {
                    continue;
                }

                ReadTable(connection, table, siteName, records);
            }

            return records
                .OrderByDescending(record => record.Timestamp)
                .Take(limit)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read bet history from SQLite.");
            return [];
        }
    }

    private static bool TableExists(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "select count(*) from sqlite_master where type = 'table' and name = $table";
        command.Parameters.AddWithValue("$table", table);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static void ReadTable(
        SqliteConnection connection,
        string table,
        string? siteName,
        ICollection<BetHistoryRecord> records)
    {
        var columns = GetColumns(connection, table);
        var selectColumns = new[]
        {
            "Date",
            "Site",
            "Game",
            "Currency",
            "TotalAmount",
            "Profit",
            "IsWin"
        }.Concat(GetOptionalFairnessColumns(columns));

        using var command = connection.CreateCommand();
        command.CommandText = string.IsNullOrWhiteSpace(siteName)
            ? $"select {string.Join(", ", selectColumns)} from {table}"
            : $"select {string.Join(", ", selectColumns)} from {table} where Site = $site";

        if (!string.IsNullOrWhiteSpace(siteName))
        {
            command.Parameters.AddWithValue("$site", siteName);
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            records.Add(new BetHistoryRecord(
                ReadTimestamp(reader.GetString(0)),
                reader.GetString(1),
                ReadGameName(reader.GetInt32(2), table),
                reader.GetString(3),
                ReadDecimal(reader.GetString(4)),
                ReadDecimal(reader.GetString(5)),
                reader.GetBoolean(6) ? "Win" : "Loss",
                ReadOptionalString(reader, columns, "ServerSeed"),
                ReadOptionalString(reader, columns, "ClientSeed"),
                ReadOptionalInt64(reader, columns, "Nonce")));
        }
    }

    private static HashSet<string> GetColumns(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"pragma table_info({table})";
        using var reader = command.ExecuteReader();
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static IEnumerable<string> GetOptionalFairnessColumns(IReadOnlySet<string> columns)
    {
        if (columns.Contains("ServerSeed"))
        {
            yield return "ServerSeed";
        }

        if (columns.Contains("ClientSeed"))
        {
            yield return "ClientSeed";
        }

        if (columns.Contains("Nonce"))
        {
            yield return "Nonce";
        }
    }

    private static string? FindDatabasePath()
    {
        foreach (var candidate in GetDatabaseCandidates())
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetDatabaseCandidates()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "GamblerBot.db");
        yield return Path.Combine(Environment.CurrentDirectory, "GamblerBot.db");
        yield return Path.Combine(Environment.CurrentDirectory, "Gambler.Bot", "GamblerBot.db");

        var directory = new DirectoryInfo(Environment.CurrentDirectory);
        while (directory is not null)
        {
            yield return Path.Combine(directory.FullName, "Gambler.Bot", "GamblerBot.db");
            directory = directory.Parent;
        }
    }

    private static DateTimeOffset ReadTimestamp(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var epochSeconds)
            ? DateTimeOffset.FromUnixTimeMilliseconds((long)(epochSeconds * 1000m))
            : DateTimeOffset.MinValue;
    }

    private static decimal ReadDecimal(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result) ? result : 0m;
    }

    private static string? ReadOptionalString(SqliteDataReader reader, IReadOnlySet<string> columns, string column)
    {
        if (!columns.Contains(column))
        {
            return null;
        }

        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static long? ReadOptionalInt64(SqliteDataReader reader, IReadOnlySet<string> columns, string column)
    {
        if (!columns.Contains(column))
        {
            return null;
        }

        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    private static string ReadGameName(int gameValue, string table)
    {
        return table switch
        {
            "DiceBets" => "Dice",
            "LimboBets" => "Limbo",
            "TwistBets" => "Twist",
            "CrashBets" => "Crash",
            "PlinkoBets" => "Plinko",
            "RouletteBets" => "Roulette",
            _ => gameValue.ToString()
        };
    }
}
