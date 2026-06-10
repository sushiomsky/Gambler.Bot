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
        _navigationContext = e.Parameter as NavigationContext;
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

    private void ShowFailure(string message)
    {
        VerifierInfoBar.Severity = InfoBarSeverity.Warning;
        VerifierInfoBar.Title = "Missing input";
        VerifierInfoBar.Message = message;
    }
}
