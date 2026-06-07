using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class BetHistoryPage : Page
{
    private NavigationContext? _navigationContext;

    public BetHistoryPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _navigationContext = e.Parameter as NavigationContext;
        LoadHistory();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadHistory();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        HistoryInfoBar.Severity = InfoBarSeverity.Informational;
        HistoryInfoBar.Title = "Export pending";
        HistoryInfoBar.Message = "CSV export will be wired after persisted bet storage is attached.";
    }

    private void LoadHistory()
    {
        if (_navigationContext is null)
        {
            return;
        }

        var activeSite = _navigationContext.SiteSessionService.Current.SelectedSite?.Name;
        var records = _navigationContext.BetHistoryService.GetRecent(activeSite);
        HistoryListView.ItemsSource = records;
        HistorySubtitleText.Text = activeSite is null
            ? $"{records.Count} records loaded across all sites."
            : $"{records.Count} records loaded for {activeSite}.";
        HistoryInfoBar.Message = records.Count == 0
            ? "No persisted bets found yet."
            : "Persisted bets loaded from SQLite.";
    }
}
