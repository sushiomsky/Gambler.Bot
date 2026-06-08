using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class BetHistoryPage : Page
{
    private NavigationContext? _navigationContext;
    private IReadOnlyList<BetHistoryRecord> _allRecords = [];
    private IReadOnlyList<BetHistoryRecord> _filteredRecords = [];

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

        if (_filteredRecords.Count == 0)
        {
            HistoryInfoBar.Severity = InfoBarSeverity.Warning;
            HistoryInfoBar.Title = "Nothing to export";
            HistoryInfoBar.Message = "Load bet history or change filters before exporting.";
            return;
        }

        try
        {
            var path = await _navigationContext.BetHistoryExportService.ExportCsvAsync(_filteredRecords);
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

    private void FilterControl_Changed(object sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    private void LoadHistory()
    {
        if (_navigationContext is null)
        {
            return;
        }

        var activeSite = _navigationContext.SiteSessionService.Current.SelectedSite?.Name;
        _allRecords = _navigationContext.BetHistoryService.GetRecent(activeSite);
        ApplyFilters();
        HistorySubtitleText.Text = activeSite is null
            ? $"{_allRecords.Count} records loaded across all sites."
            : $"{_allRecords.Count} records loaded for {activeSite}.";
        HistoryInfoBar.Message = _allRecords.Count == 0
            ? "No persisted bets found yet."
            : "Persisted bets loaded from SQLite.";
    }

    private void ApplyFilters()
    {
        if (_navigationContext is null)
        {
            return;
        }

        var outcome = (OutcomeFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        _filteredRecords = _navigationContext.BetHistoryFilterService.Apply(_allRecords, SearchTextBox.Text, outcome);
        HistoryListView.ItemsSource = _filteredRecords;
        HistorySubtitleText.Text = $"{_filteredRecords.Count} of {_allRecords.Count} records visible.";
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        if (_navigationContext is null)
        {
            return;
        }

        var summary = _navigationContext.BetHistorySummaryService.Summarize(_filteredRecords);
        VisibleRecordsText.Text = summary.TotalRecords.ToString(CultureInfo.InvariantCulture);
        WinLossText.Text = $"{summary.Wins} / {summary.Losses}";
        WinRateText.Text = $"{summary.WinRate.ToString(CultureInfo.InvariantCulture)}%";
        WageredText.Text = summary.TotalAmount.ToString(CultureInfo.InvariantCulture);
        NetProfitText.Text = summary.NetProfit.ToString(CultureInfo.InvariantCulture);
        UpdateChart();
    }

    private void UpdateChart()
    {
        if (_navigationContext is null)
        {
            return;
        }

        var chart = _navigationContext.BetChartService.CreateSnapshot(_filteredRecords);
        ProfitSparklineText.Text = chart.Sparkline;
        ChartEndProfitText.Text = chart.EndProfit.ToString(CultureInfo.InvariantCulture);
        ChartBestWorstText.Text = $"{chart.BestProfit.ToString(CultureInfo.InvariantCulture)} / {chart.WorstProfit.ToString(CultureInfo.InvariantCulture)}";
        ChartWinLossText.Text = $"{chart.Wins} / {chart.Losses}";
    }
}
