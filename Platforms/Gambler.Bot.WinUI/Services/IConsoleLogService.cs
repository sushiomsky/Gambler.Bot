using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IConsoleLogService
{
    event EventHandler<ConsoleLogEntry>? EntryAdded;
    IReadOnlyList<ConsoleLogEntry> Entries { get; }
    void Log(string level, string message);
    void Clear();
}
