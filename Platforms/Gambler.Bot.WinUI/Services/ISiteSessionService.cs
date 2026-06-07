using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface ISiteSessionService
{
    event EventHandler<SiteSessionState>? StateChanged;
    SiteSessionState Current { get; }
    void Select(SiteSummary site);
    void StartSimulation(SiteSummary site);
    void SetLiveConnected(SiteSummary site);
    void Clear();
}
