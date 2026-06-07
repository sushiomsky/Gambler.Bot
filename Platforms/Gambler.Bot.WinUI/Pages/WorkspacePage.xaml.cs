using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class WorkspacePage : Page
{
    public WorkspacePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var area = e.Parameter as string ?? "workspace";
        TitleText.Text = ToTitle(area);
        SubtitleText.Text = GetSubtitle(area);
        BodyText.Text = GetBody(area);
    }

    private static string ToTitle(string area) => area switch
    {
        "sites" => "Sites",
        "strategies" => "Strategies",
        "history" => "Bet History",
        "intelligence" => "Intelligence",
        _ => "Workspace"
    };

    private static string GetSubtitle(string area) => area switch
    {
        "sites" => "Connectors, login state, balances, and site-specific capabilities.",
        "strategies" => "Preset strategies, programmer mode, validation, and execution control.",
        "history" => "Searchable session timeline, outcomes, wagers, and export paths.",
        "intelligence" => "Risk limits, anomaly detection, and session summaries.",
        _ => "Native workspace prepared for migration."
    };

    private static string GetBody(string area) => area switch
    {
        "sites" => "This will become the replacement for the existing site selector and account status surfaces.",
        "strategies" => "This is where the current martingale, fibonacci, labouchere, d'Alembert, preset list, and programmer-mode screens move next.",
        "history" => "This will replace the bet history and session stats views with a dense native table and filters.",
        "intelligence" => "This area gives the bot a smarter interface: warnings, bankroll guardrails, and strategy diagnostics.",
        _ => "This placeholder keeps navigation stable while each Avalonia screen is ported."
    };
}
