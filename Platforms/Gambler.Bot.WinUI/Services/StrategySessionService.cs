using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class StrategySessionService : IStrategySessionService
{
    public event EventHandler<StrategySessionState>? StateChanged;

    public StrategySessionState Current { get; private set; } = StrategySessionState.Empty;

    public void Select(StrategySummary strategy)
    {
        SetState(new StrategySessionState(strategy));
    }

    public void Clear()
    {
        SetState(StrategySessionState.Empty);
    }

    private void SetState(StrategySessionState state)
    {
        Current = state;
        StateChanged?.Invoke(this, state);
    }
}
