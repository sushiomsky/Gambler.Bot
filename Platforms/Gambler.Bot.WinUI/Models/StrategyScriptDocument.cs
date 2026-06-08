namespace Gambler.Bot.WinUI.Models;

public sealed record StrategyScriptDocument(
    StrategySummary Strategy,
    string Runtime,
    string FileExtension,
    string FilePath,
    string Content,
    DateTimeOffset LastSavedAt);
