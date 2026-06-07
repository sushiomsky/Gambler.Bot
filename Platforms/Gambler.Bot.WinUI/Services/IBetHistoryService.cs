using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IBetHistoryService
{
    IReadOnlyList<BetHistoryRecord> GetRecent(string? siteName = null, int limit = 100);
}
