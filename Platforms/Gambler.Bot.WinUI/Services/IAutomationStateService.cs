using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IAutomationStateService
{
    event EventHandler<AutomationState>? StateChanged;
    AutomationState Current { get; }
    void Start();
    void Pause();
    void Stop();
}
