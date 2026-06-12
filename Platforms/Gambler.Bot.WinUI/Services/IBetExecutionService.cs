using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IBetExecutionService
{
    AutomationCommandResult PrepareNextBet(out PlaceBetPreview? preview);
    Task<AutomationCommandResult> ExecuteLiveBetAsync(CancellationToken cancellationToken = default);
}
