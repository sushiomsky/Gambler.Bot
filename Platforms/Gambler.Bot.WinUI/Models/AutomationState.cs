namespace Gambler.Bot.WinUI.Models;

public sealed record AutomationState(
    string Status,
    DateTimeOffset? StartedAt,
    string Mode = "Idle",
    int LoopIterations = 0,
    string LastMessage = "Idle.",
    PlaceBetPreview? LastBetPreview = null)
{
    public static AutomationState Idle { get; } = new("Idle", null);
}
