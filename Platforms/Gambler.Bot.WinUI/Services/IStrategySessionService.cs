using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IStrategySessionService
{
    event EventHandler<StrategySessionState>? StateChanged;
    StrategySessionState Current { get; }
    void Select(StrategySummary strategy);
    void Clear();
}
