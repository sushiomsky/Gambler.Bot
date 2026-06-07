using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class AutomationStateService : IAutomationStateService
{
    public event EventHandler<AutomationState>? StateChanged;

    public AutomationState Current { get; private set; } = AutomationState.Idle;

    public void Start()
    {
        SetState(new AutomationState("Running", DateTimeOffset.Now));
    }

    public void Pause()
    {
        SetState(Current with { Status = "Paused" });
    }

    public void Stop()
    {
        SetState(AutomationState.Idle);
    }

    private void SetState(AutomationState state)
    {
        Current = state;
        StateChanged?.Invoke(this, state);
    }
}
