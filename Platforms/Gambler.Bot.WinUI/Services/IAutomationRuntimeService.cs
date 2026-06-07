using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IAutomationRuntimeService
{
    AutomationCommandResult Start();
    AutomationCommandResult Pause();
    AutomationCommandResult Stop();
}
