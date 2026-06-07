namespace Gambler.Bot.WinUI.Models;

public sealed record StrategySummary(
    string Name,
    string Kind,
    string Runtime,
    string Status,
    string StrategyTypeName);
