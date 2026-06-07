using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface ILiveLoginService
{
    Task<AutomationCommandResult> LoginAsync(LoginProfile profile, CancellationToken cancellationToken = default);
}
