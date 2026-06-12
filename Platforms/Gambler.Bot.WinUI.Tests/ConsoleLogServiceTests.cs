using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class ConsoleLogServiceTests
{
    [Fact]
    public void LogAddsEntryAndRaisesEvent()
    {
        var service = new ConsoleLogService();
        var eventCount = 0;
        service.EntryAdded += (_, entry) =>
        {
            eventCount++;
            Assert.Equal("Info", entry.Level);
        };

        service.Log("Info", "Console ready");

        Assert.Equal(1, eventCount);
        Assert.Single(service.Entries);
        Assert.Contains("Console ready", service.Entries[0].DisplayText);
    }

    [Fact]
    public void ClearRemovesEntries()
    {
        var service = new ConsoleLogService();
        service.Log("Info", "One");
        service.Log("Warn", "Two");

        service.Clear();

        Assert.Empty(service.Entries);
    }

    [Fact]
    public void LogKeepsMostRecentFiveHundredEntries()
    {
        var service = new ConsoleLogService();

        for (var index = 0; index < 510; index++)
        {
            service.Log("Info", $"Entry {index}");
        }

        Assert.Equal(500, service.Entries.Count);
        Assert.Contains("Entry 10", service.Entries[0].DisplayText);
        Assert.Contains("Entry 509", service.Entries[^1].DisplayText);
    }

    [Fact]
    public void LogPersistsEntriesToDisk()
    {
        var logPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "ConsoleLog.jsonl");
        var service = new ConsoleLogService(logPath: logPath);

        service.Log("Info", "Persist me");

        var reloaded = new ConsoleLogService(logPath: logPath);
        Assert.Single(reloaded.Entries);
        Assert.Contains("Persist me", reloaded.Entries[0].DisplayText);
    }

    [Fact]
    public void PersistedLogKeepsConfiguredRetention()
    {
        var logPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "ConsoleLog.jsonl");
        var service = new ConsoleLogService(logPath: logPath, retentionEntries: 3);

        for (var index = 0; index < 5; index++)
        {
            service.Log("Info", $"Entry {index}");
        }

        var reloaded = new ConsoleLogService(logPath: logPath, retentionEntries: 3);
        Assert.Equal(3, reloaded.Entries.Count);
        Assert.Contains("Entry 2", reloaded.Entries[0].DisplayText);
        Assert.Contains("Entry 4", reloaded.Entries[^1].DisplayText);
    }

    [Fact]
    public void ClearRemovesPersistedLogFile()
    {
        var logPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "ConsoleLog.jsonl");
        var service = new ConsoleLogService(logPath: logPath);
        service.Log("Info", "Delete me");

        service.Clear();

        Assert.Empty(service.Entries);
        Assert.False(File.Exists(logPath));
    }
}
