using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IBetChartService
{
    BetChartSnapshot CreateSnapshot(IReadOnlyList<BetHistoryRecord> records);
}
