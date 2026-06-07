namespace Gambler.Bot.WinUI.Models;

public sealed record StrategySessionState(StrategySummary? SelectedStrategy)
{
    public static StrategySessionState Empty { get; } = new((StrategySummary?)null);
}
