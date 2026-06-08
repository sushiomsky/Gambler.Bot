using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class AutomationStateService : IAutomationStateService
{
    private readonly object _gate = new();

    public event EventHandler<AutomationState>? StateChanged;

    public AutomationState Current { get; private set; } = AutomationState.Idle;

    public void Start(string mode = "Simulation", string message = "Runtime started.")
    {
        SetState(new AutomationState("Running", DateTimeOffset.Now, mode, 0, message));
    }

    public void Pause()
    {
        SetState(Current with { Status = "Paused", LastMessage = "Runtime paused." });
    }

    public void RecordIteration(string message, PlaceBetPreview? preview)
    {
        SetState(Current with
        {
            LoopIterations = Current.LoopIterations + 1,
            LastMessage = message,
            LastBetPreview = preview
        });
    }

    public void Complete(string message)
    {
        SetState(Current with { Status = "Completed", LastMessage = message });
    }

    public void Stop()
    {
        SetState(AutomationState.Idle);
    }

    private void SetState(AutomationState state)
    {
        lock (_gate)
        {
            Current = state;
        }

        StateChanged?.Invoke(this, state);
    }
}
