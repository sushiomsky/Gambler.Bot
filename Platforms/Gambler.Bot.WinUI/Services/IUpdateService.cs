using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IUpdateService
{
    Task<UpdateStatus> GetStatusAsync(CancellationToken cancellationToken = default);
}
