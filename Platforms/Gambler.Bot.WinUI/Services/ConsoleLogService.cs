using System.Text.Json;
using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class ConsoleLogService : IConsoleLogService
{
    private const int MaximumEntries = 500;
    private readonly object _gate = new();
    private readonly List<ConsoleLogEntry> _entries = [];
    private readonly string _logPath;
    private readonly int _retentionEntries;

    public event EventHandler<ConsoleLogEntry>? EntryAdded;

    public ConsoleLogService(
        IAppSettingsService? appSettingsService = null,
        string? logPath = null,
        int? retentionEntries = null)
    {
        _retentionEntries = Math.Clamp(retentionEntries ?? LoadRetentionEntries(appSettingsService), 1, 10_000);
        _logPath = string.IsNullOrWhiteSpace(logPath) ? GetDefaultLogPath() : logPath;
        LoadPersistedEntries();
    }

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
            TrimEntries();
            PersistEntries();
        }

        EntryAdded?.Invoke(this, entry);
    }

    public void Clear()
    {
        lock (_gate)
        {
            _entries.Clear();
            if (File.Exists(_logPath))
            {
                File.Delete(_logPath);
            }
        }
    }

    private static string Normalize(string level)
    {
        return string.IsNullOrWhiteSpace(level) ? "Info" : level.Trim();
    }

    private static int LoadRetentionEntries(IAppSettingsService? appSettingsService)
    {
        if (appSettingsService is null)
        {
            return MaximumEntries;
        }

        try
        {
            return appSettingsService.LoadAsync().GetAwaiter().GetResult().ConsoleRetentionEntries;
        }
        catch
        {
            return MaximumEntries;
        }
    }

    private static string GetDefaultLogPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "Gambler.Bot", "ConsoleLog.jsonl");
    }

    private void LoadPersistedEntries()
    {
        if (!File.Exists(_logPath))
        {
            return;
        }

        foreach (var line in File.ReadLines(_logPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var entry = JsonSerializer.Deserialize<ConsoleLogEntry>(line);
                if (entry is not null)
                {
                    _entries.Add(entry);
                }
            }
            catch (JsonException)
            {
                // Keep startup resilient if a previous run left a partial log line.
            }
        }

        TrimEntries();
        PersistEntries();
    }

    private void TrimEntries()
    {
        if (_entries.Count > _retentionEntries)
        {
            _entries.RemoveRange(0, _entries.Count - _retentionEntries);
        }
    }

    private void PersistEntries()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
        File.WriteAllLines(_logPath, _entries.Select(entry => JsonSerializer.Serialize(entry)));
    }
}
