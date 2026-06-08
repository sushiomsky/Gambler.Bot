using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IStrategyScriptService
{
    bool CanEdit(StrategySummary strategy);
    string GetDefaultScriptPath(StrategySummary strategy);
    Task<StrategyScriptDocument> OpenOrCreateAsync(StrategySummary strategy, CancellationToken cancellationToken = default);
    Task<StrategyScriptDocument> SaveAsync(StrategyScriptDocument document, string content, CancellationToken cancellationToken = default);
    AutomationCommandResult Validate(StrategyScriptDocument document);
}
