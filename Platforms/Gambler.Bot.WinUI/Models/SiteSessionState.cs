using Gambler.Bot.Core.Sites;

namespace Gambler.Bot.WinUI.Models;

public sealed record SiteSessionState(
    SiteSummary? SelectedSite,
    string Mode,
    bool IsConnected,
    BaseSite? RuntimeSite = null)
{
    public static SiteSessionState Empty { get; } = new(null, "Idle", false);
}
