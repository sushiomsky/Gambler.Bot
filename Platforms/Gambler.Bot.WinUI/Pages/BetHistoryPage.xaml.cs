using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class BetHistoryPage : Page
{
    private NavigationContext? _navigationContext;
    private IReadOnlyList<BetHistoryRecord> _records = [];

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

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null)
        {
            return;
        }

        if (_records.Count == 0)
        {
            HistoryInfoBar.Severity = InfoBarSeverity.Warning;
            HistoryInfoBar.Title = "Nothing to export";
            HistoryInfoBar.Message = "Load bet history before exporting.";
            return;
        }

        try
        {
            var path = await _navigationContext.BetHistoryExportService.ExportCsvAsync(_records);
            HistoryInfoBar.Severity = InfoBarSeverity.Success;
            HistoryInfoBar.Title = "Export complete";
            HistoryInfoBar.Message = $"CSV exported to {path}.";
        }
        catch (Exception ex)
        {
            HistoryInfoBar.Severity = InfoBarSeverity.Error;
            HistoryInfoBar.Title = "Export failed";
            HistoryInfoBar.Message = ex.Message;
        }
    }

    private void LoadHistory()
    {
        if (_navigationContext is null)
        {
            return;
        }

        var activeSite = _navigationContext.SiteSessionService.Current.SelectedSite?.Name;
        _records = _navigationContext.BetHistoryService.GetRecent(activeSite);
        HistoryListView.ItemsSource = _records;
        HistorySubtitleText.Text = activeSite is null
            ? $"{_records.Count} records loaded across all sites."
            : $"{_records.Count} records loaded for {activeSite}.";
        HistoryInfoBar.Message = _records.Count == 0
            ? "No persisted bets found yet."
            : "Persisted bets loaded from SQLite.";
    }
}
