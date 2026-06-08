using System.Globalization;
using System.Text;
using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class BetHistoryExportService : IBetHistoryExportService
{
    public async Task<string> ExportCsvAsync(
        IReadOnlyList<BetHistoryRecord> records,
        string? filePath = null,
        CancellationToken cancellationToken = default)
    {
        var destination = string.IsNullOrWhiteSpace(filePath) ? CreateDefaultFilePath() : filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        await using var stream = File.Create(destination);
        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        await writer.WriteLineAsync("Timestamp,Site,Game,Currency,Amount,Profit,Outcome");
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
                Escape(record.Outcome)));
        }

        return destination;
    }

    private static string CreateDefaultFilePath()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var fileName = $"BetHistory-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.csv";
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
}
