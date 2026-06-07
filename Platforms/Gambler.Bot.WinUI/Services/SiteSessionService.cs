using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class SiteSessionService : ISiteSessionService
{
    public event EventHandler<SiteSessionState>? StateChanged;

    public SiteSessionState Current { get; private set; } = SiteSessionState.Empty;

    public void Select(SiteSummary site)
    {
        SetState(new SiteSessionState(site, "Selected", false));
    }

    public void StartSimulation(SiteSummary site)
    {
        SetState(new SiteSessionState(site, "Simulation", true));
    }

    public void SetLiveConnected(SiteSummary site)
    {
        SetState(new SiteSessionState(site, "Live", true));
    }

    public void Clear()
    {
        SetState(SiteSessionState.Empty);
    }

    private void SetState(SiteSessionState state)
    {
        Current = state;
        StateChanged?.Invoke(this, state);
    }
}
