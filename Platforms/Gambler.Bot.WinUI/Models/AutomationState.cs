namespace Gambler.Bot.WinUI.Models;

public sealed record AutomationState(string Status, DateTimeOffset? StartedAt)
{
    public static AutomationState Idle { get; } = new("Idle", null);
}
