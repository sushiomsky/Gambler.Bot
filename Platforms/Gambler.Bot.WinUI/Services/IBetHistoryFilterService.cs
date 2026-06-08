using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IBetHistoryFilterService
{
    IReadOnlyList<BetHistoryRecord> Apply(
        IReadOnlyList<BetHistoryRecord> records,
        string? searchText,
        string? outcome);
}
