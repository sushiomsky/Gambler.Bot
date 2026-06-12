using System.Globalization;
using System.Text;
using System.Text.Json;
using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class BetHistoryExportService : IBetHistoryExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public Task<string> ExportAsync(
        IReadOnlyList<BetHistoryRecord> records,
        BetHistoryExportFormat format,
        BetHistorySummary? summary = null,
        BetChartSnapshot? chart = null,
        string? filePath = null,
        CancellationToken cancellationToken = default)
    {
        return format switch
        {
            BetHistoryExportFormat.Csv => ExportCsvAsync(records, filePath, cancellationToken),
            BetHistoryExportFormat.Json => ExportJsonAsync(records, summary, chart, filePath, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported bet history export format.")
        };
    }

    public async Task<string> ExportCsvAsync(
        IReadOnlyList<BetHistoryRecord> records,
        string? filePath = null,
        CancellationToken cancellationToken = default)
    {
        var destination = string.IsNullOrWhiteSpace(filePath) ? CreateDefaultFilePath("csv") : filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        await using var stream = File.Create(destination);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        await writer.WriteLineAsync("Timestamp,Site,Game,Currency,Amount,Profit,Outcome,ServerSeed,ClientSeed,Nonce");
        foreach (var record in records)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await writer.WriteLineAsync(string.Join(
                ",",
                Escape(record.Timestamp.ToString("O", CultureInfo.InvariantCulture)),
                Escape(record.Site),
                Escape(record.Game),
                Escape(record.Currency),
                Escape(record.Amount.ToString(CultureInfo.InvariantCulture)),
                Escape(record.Profit.ToString(CultureInfo.InvariantCulture)),
                Escape(record.Outcome),
                Escape(record.ServerSeed ?? string.Empty),
                Escape(record.ClientSeed ?? string.Empty),
                Escape(record.Nonce?.ToString(CultureInfo.InvariantCulture) ?? string.Empty)));
        }

        return destination;
    }

    private static async Task<string> ExportJsonAsync(
        IReadOnlyList<BetHistoryRecord> records,
        BetHistorySummary? summary,
        BetChartSnapshot? chart,
        string? filePath,
        CancellationToken cancellationToken)
    {
        var destination = string.IsNullOrWhiteSpace(filePath) ? CreateDefaultFilePath("json") : filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        var payload = new BetHistoryExportPayload(
            DateTimeOffset.Now,
            records.Count,
            summary,
            chart,
            records);

        await using var stream = File.Create(destination);
        await JsonSerializer.SerializeAsync(stream, payload, JsonOptions, cancellationToken);
        return destination;
    }

    private static string CreateDefaultFilePath(string extension)
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var fileName = $"BetHistory-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.{extension}";
        return Path.Combine(documents, "Gambler.Bot", fileName);
    }

    private static string Escape(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\r') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private sealed record BetHistoryExportPayload(
        DateTimeOffset ExportedAt,
        int RecordCount,
        BetHistorySummary? Summary,
        BetChartSnapshot? Chart,
        IReadOnlyList<BetHistoryRecord> Records);
}
