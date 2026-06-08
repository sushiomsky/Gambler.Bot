namespace Gambler.Bot.WinUI.Models;

public sealed record ConsoleLogEntry(
    DateTimeOffset Timestamp,
    string Level,
    string Message)
{
    public string DisplayText => $"[{Timestamp:HH:mm:ss}] {Level}: {Message}";
}
