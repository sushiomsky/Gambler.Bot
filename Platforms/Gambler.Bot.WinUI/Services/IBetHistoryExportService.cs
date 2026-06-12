using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IBetHistoryExportService
{
    Task<string> ExportAsync(
        IReadOnlyList<BetHistoryRecord> records,
        BetHistoryExportFormat format,
        BetHistorySummary? summary = null,
        BetChartSnapshot? chart = null,
        string? filePath = null,
        CancellationToken cancellationToken = default);

    Task<string> ExportCsvAsync(
        IReadOnlyList<BetHistoryRecord> records,
        string? filePath = null,
        CancellationToken cancellationToken = default);
}
