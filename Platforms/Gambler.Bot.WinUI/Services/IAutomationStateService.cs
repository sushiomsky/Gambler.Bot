using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IAutomationStateService
{
    event EventHandler<AutomationState>? StateChanged;
    AutomationState Current { get; }
    void Start(string mode = "Simulation", string message = "Runtime started.");
    void Pause();
    void RecordIteration(string message, PlaceBetPreview? preview);
    void Complete(string message);
    void Stop();
}
