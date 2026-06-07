namespace Gambler.Bot.WinUI.Models;

public sealed record SiteSessionState(
    SiteSummary? SelectedSite,
    string Mode,
    bool IsConnected)
{
    public static SiteSessionState Empty { get; } = new(null, "Idle", false);
}
