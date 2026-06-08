using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class BetHistorySummaryService : IBetHistorySummaryService
{
    public BetHistorySummary Summarize(IReadOnlyList<BetHistoryRecord> records)
    {
        return new BetHistorySummary(
            records.Count,
            records.Count(record => string.Equals(record.Outcome, "Win", StringComparison.OrdinalIgnoreCase)),
            records.Count(record => string.Equals(record.Outcome, "Loss", StringComparison.OrdinalIgnoreCase)),
            records.Sum(record => record.Amount),
            records.Sum(record => record.Profit));
    }
}
