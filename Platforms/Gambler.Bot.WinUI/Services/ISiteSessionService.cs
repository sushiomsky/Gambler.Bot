using Gambler.Bot.WinUI.Models;
using Gambler.Bot.Core.Sites;

namespace Gambler.Bot.WinUI.Services;

public interface ISiteSessionService
{
    event EventHandler<SiteSessionState>? StateChanged;
    SiteSessionState Current { get; }
    void Select(SiteSummary site);
    void StartSimulation(SiteSummary site);
    void SetLiveConnected(SiteSummary site, BaseSite? runtimeSite = null);
    void Clear();
}
