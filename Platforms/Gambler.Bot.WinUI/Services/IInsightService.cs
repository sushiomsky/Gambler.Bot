using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IInsightService
{
    Task<IReadOnlyList<InsightItem>> GetInsightsAsync(CancellationToken cancellationToken = default);
}
