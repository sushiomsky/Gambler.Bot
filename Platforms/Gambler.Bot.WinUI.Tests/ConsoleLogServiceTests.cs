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
}
