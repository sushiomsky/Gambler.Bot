using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IBetHistorySummaryService
{
    BetHistorySummary Summarize(IReadOnlyList<BetHistoryRecord> records);
}
