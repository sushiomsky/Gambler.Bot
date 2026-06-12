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
            var summary = _navigationContext.BetHistorySummaryService.Summarize(_filteredRecords);
            var chart = _navigationContext.BetChartService.CreateSnapshot(_filteredRecords);
            var format = ReadExportFormat();
            var path = await _navigationContext.BetHistoryExportService.ExportAsync(_filteredRecords, format, summary, chart);
            HistoryInfoBar.Severity = InfoBarSeverity.Success;
            HistoryInfoBar.Title = "Export complete";
            HistoryInfoBar.Message = $"{format} exported to {path}.";
        }
        catch (Exception ex)
        {
            HistoryInfoBar.Severity = InfoBarSeverity.Error;
            HistoryInfoBar.Title = "Export failed";
            HistoryInfoBar.Message = ex.Message;
        }
    }

    private void VerifySelectedButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null)
        {
            return;
        }

        if (HistoryListView.SelectedItem is not BetHistoryRecord record)
        {
            HistoryInfoBar.Severity = InfoBarSeverity.Warning;
            HistoryInfoBar.Title = "No bet selected";
            HistoryInfoBar.Message = "Select a history row before opening the verifier.";
            return;
        }

        if (!record.CanPrefillVerifier)
        {
            HistoryInfoBar.Severity = InfoBarSeverity.Warning;
            HistoryInfoBar.Title = "Verifier data unavailable";
            HistoryInfoBar.Message = "The selected bet does not include server seed, client seed, and nonce data.";
            return;
        }

        Frame.Navigate(typeof(RollVerifierPage), new RollVerifierNavigationRequest(_navigationContext, record));
    }

    private void FilterControl_Changed(object sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSelectedBetDetails(HistoryListView.SelectedItem as BetHistoryRecord);
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
        var criteria = new BetHistoryFilterCriteria(
            SearchTextBox.Text,
            outcome,
            CurrencyFilterTextBox.Text,
            ReadOptionalDecimal(MinimumProfitTextBox.Text),
            ReadOptionalDecimal(MaximumProfitTextBox.Text),
            VerifierReadyOnlyCheckBox.IsChecked == true);
        _filteredRecords = _navigationContext.BetHistoryFilterService.Apply(_allRecords, criteria);
        HistoryListView.ItemsSource = _filteredRecords;
        if (HistoryListView.SelectedItem is BetHistoryRecord selected && !_filteredRecords.Contains(selected))
        {
            HistoryListView.SelectedItem = null;
        }

        HistorySubtitleText.Text = $"{_filteredRecords.Count} of {_allRecords.Count} records visible.";
        UpdateSummary();
        UpdateSelectedBetDetails(HistoryListView.SelectedItem as BetHistoryRecord);
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
        ChartRoiText.Text = $"{chart.ReturnOnInvestmentPercent.ToString("0.##", CultureInfo.InvariantCulture)}%";
        ChartAverageProfitText.Text = chart.AverageProfit.ToString("0.########", CultureInfo.InvariantCulture);
        ChartDrawdownText.Text = chart.MaximumDrawdown.ToString("0.########", CultureInfo.InvariantCulture);
        ChartStreakText.Text = $"{chart.LongestWinStreak} / {chart.LongestLossStreak}";
    }

    private static decimal? ReadOptionalDecimal(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    private BetHistoryExportFormat ReadExportFormat()
    {
        var selected = (ExportFormatComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        return string.Equals(selected, "JSON with summary", StringComparison.OrdinalIgnoreCase)
            ? BetHistoryExportFormat.Json
            : BetHistoryExportFormat.Csv;
    }

    private void UpdateSelectedBetDetails(BetHistoryRecord? record)
    {
        if (_navigationContext is null || record is null)
        {
            SelectedBetHintText.Text = "Select a history row to inspect all stored bet fields.";
            SelectedBetDetailsRepeater.ItemsSource = Array.Empty<BetHistoryDetailItem>();
            return;
        }

        SelectedBetHintText.Text = record.CanPrefillVerifier
            ? "This bet contains verifier seed data and can be opened in Roll Verifier."
            : "This bet does not include enough seed data for automatic verification.";
        SelectedBetDetailsRepeater.ItemsSource = _navigationContext.BetHistoryDetailService.CreateDetails(record);
    }
}
