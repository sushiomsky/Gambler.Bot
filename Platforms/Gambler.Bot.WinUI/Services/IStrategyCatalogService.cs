using Gambler.Bot.WinUI.Models;
using Gambler.Bot.Strategies.Strategies.Abstractions;

namespace Gambler.Bot.WinUI.Services;

public interface IStrategyCatalogService
{
    IReadOnlyList<StrategySummary> GetStrategies();
    BaseStrategy? CreateStrategy(StrategySummary summary);
}
