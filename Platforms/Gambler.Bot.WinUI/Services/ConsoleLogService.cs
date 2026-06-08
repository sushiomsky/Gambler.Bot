using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class ConsoleLogService : IConsoleLogService
{
    private const int MaximumEntries = 500;
    private readonly object _gate = new();
    private readonly List<ConsoleLogEntry> _entries = [];

    public event EventHandler<ConsoleLogEntry>? EntryAdded;

    public IReadOnlyList<ConsoleLogEntry> Entries
    {
        get
        {
            lock (_gate)
            {
                return _entries.ToList();
            }
        }
    }

    public void Log(string level, string message)
    {
        var entry = new ConsoleLogEntry(DateTimeOffset.Now, Normalize(level), message.Trim());
        lock (_gate)
        {
            _entries.Add(entry);
            if (_entries.Count > MaximumEntries)
            {
                _entries.RemoveRange(0, _entries.Count - MaximumEntries);
            }
        }

        EntryAdded?.Invoke(this, entry);
    }

    public void Clear()
    {
        lock (_gate)
        {
            _entries.Clear();
        }
    }

    private static string Normalize(string level)
    {
        return string.IsNullOrWhiteSpace(level) ? "Info" : level.Trim();
    }
}
