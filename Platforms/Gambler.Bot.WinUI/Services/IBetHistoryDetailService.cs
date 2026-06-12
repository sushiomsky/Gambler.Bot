using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IBetHistoryDetailService
{
    IReadOnlyList<BetHistoryDetailItem> CreateDetails(BetHistoryRecord record);
}
