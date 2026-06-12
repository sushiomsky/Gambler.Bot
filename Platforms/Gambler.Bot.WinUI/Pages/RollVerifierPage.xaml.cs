using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class RollVerifierPage : Page
{
    private NavigationContext? _navigationContext;
    private IReadOnlyList<SiteSummary> _sites = [];

    public RollVerifierPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var sourceBet = default(BetHistoryRecord);
        if (e.Parameter is RollVerifierNavigationRequest request)
        {
            _navigationContext = request.NavigationContext;
            sourceBet = request.SourceBet;
        }
        else
        {
            _navigationContext = e.Parameter as NavigationContext;
        }

        if (_navigationContext is null)
        {
            return;
        }

        _sites = _navigationContext.SiteCatalogService.GetSites()
            .Where(site => site.Games.Count > 0)
            .ToList();
        SiteComboBox.ItemsSource = _sites;

        var activeSite = _navigationContext.SiteSessionService.Current.SelectedSite;
        SiteComboBox.SelectedItem = activeSite is null
            ? _sites.FirstOrDefault()
            : _sites.FirstOrDefault(site => site.SiteTypeName == activeSite.SiteTypeName) ?? _sites.FirstOrDefault();

        if (sourceBet is not null)
        {
            PrefillFromHistory(sourceBet);
        }
    }

    private void SiteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        GameComboBox.Items.Clear();
        if (SiteComboBox.SelectedItem is not SiteSummary site)
        {
            return;
        }

        foreach (var game in site.Games)
        {
            GameComboBox.Items.Add(game);
        }

        GameComboBox.SelectedIndex = GameComboBox.Items.Count > 0 ? 0 : -1;
    }

    private void VerifyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null || SiteComboBox.SelectedItem is not SiteSummary site)
        {
            ShowFailure("Choose a site before verifying.");
            return;
        }

        if (GameComboBox.SelectedItem is not string game)
        {
            ShowFailure("Choose a game before verifying.");
            return;
        }

        var request = new RollVerificationRequest(
            site,
            game,
            ServerSeedTextBox.Text,
            ClientSeedTextBox.Text,
            Convert.ToInt32(NonceNumberBox.Value));

        var result = _navigationContext.RollVerifierService.Verify(request);
        VerifierInfoBar.Severity = result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Warning;
        VerifierInfoBar.Title = result.Succeeded ? "Verified" : "Not verified";
        VerifierInfoBar.Message = result.Message;

        ResultSiteGameText.Text = result.Succeeded ? $"{result.Site} / {result.Game}" : "-";
        ServerSeedHashText.Text = string.IsNullOrWhiteSpace(result.ServerSeedHash) ? "-" : result.ServerSeedHash;
        ResultTypeText.Text = string.IsNullOrWhiteSpace(result.ResultType) ? "Verified value" : result.ResultType;
        ResultValueText.Text = string.IsNullOrWhiteSpace(result.ResultValue) ? "-" : result.ResultValue;
    }

    private void PrefillFromHistory(BetHistoryRecord record)
    {
        var matchingSite = _sites.FirstOrDefault(site => string.Equals(site.Name, record.Site, StringComparison.OrdinalIgnoreCase))
            ?? _sites.FirstOrDefault(site => string.Equals(site.SiteTypeName, record.Site, StringComparison.OrdinalIgnoreCase));

        if (matchingSite is not null)
        {
            SiteComboBox.SelectedItem = matchingSite;
            SelectGame(record.Game);
        }

        ServerSeedTextBox.Text = record.ServerSeed ?? string.Empty;
        ClientSeedTextBox.Text = record.ClientSeed ?? string.Empty;
        NonceNumberBox.Value = record.Nonce ?? 0;

        VerifierInfoBar.Severity = record.CanPrefillVerifier ? InfoBarSeverity.Informational : InfoBarSeverity.Warning;
        VerifierInfoBar.Title = record.CanPrefillVerifier ? "Prefilled from history" : "History data incomplete";
        VerifierInfoBar.Message = record.CanPrefillVerifier
            ? $"Loaded {record.Site} {record.Game} seed data from bet history. Review the values, then verify."
            : "The selected history record does not include all verifier inputs.";
    }

    private void SelectGame(string game)
    {
        for (var index = 0; index < GameComboBox.Items.Count; index++)
        {
            if (GameComboBox.Items[index] is string item
                && string.Equals(item, game, StringComparison.OrdinalIgnoreCase))
            {
                GameComboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private void ShowFailure(string message)
    {
        VerifierInfoBar.Severity = InfoBarSeverity.Warning;
        VerifierInfoBar.Title = "Missing input";
        VerifierInfoBar.Message = message;
    }
}
