using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IBetHistoryExportService
{
    Task<string> ExportCsvAsync(
        IReadOnlyList<BetHistoryRecord> records,
        string? filePath = null,
        CancellationToken cancellationToken = default);
}
