using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class SitesPage : Page
{
    private NavigationContext? _navigationContext;

    public SitesPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _navigationContext = e.Parameter as NavigationContext;
        LoadSites();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadSites();
    }

    private void SelectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null || SitesListView.SelectedItem is not SiteSummary site)
        {
            ShowSelectionRequired();
            return;
        }

        _navigationContext.SiteSessionService.Select(site);
        SelectionInfoBar.Severity = InfoBarSeverity.Success;
        SelectionInfoBar.Title = "Site selected";
        SelectionInfoBar.Message = $"{site.Name} is now the active site context.";
    }

    private void SimulateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null || SitesListView.SelectedItem is not SiteSummary site)
        {
            ShowSelectionRequired();
            return;
        }

        _navigationContext.SiteSessionService.StartSimulation(site);
        SelectionInfoBar.Severity = InfoBarSeverity.Success;
        SelectionInfoBar.Title = "Simulation ready";
        SelectionInfoBar.Message = $"{site.Name} is active in simulation mode.";
    }

    private void LoadSites()
    {
        if (_navigationContext is null)
        {
            return;
        }

        try
        {
            var sites = _navigationContext.SiteCatalogService.GetSites();
            SitesListView.ItemsSource = sites;
            SiteCountText.Text = $"{sites.Count} supported sites discovered from Core.";
            SelectionInfoBar.Severity = InfoBarSeverity.Informational;
            SelectionInfoBar.Title = "Catalog";
            SelectionInfoBar.Message = "Site metadata is loaded from the existing Core site classes without using Avalonia.";
        }
        catch (Exception ex)
        {
            SitesListView.ItemsSource = Array.Empty<SiteSummary>();
            SiteCountText.Text = "Site catalog could not be loaded.";
            SelectionInfoBar.Severity = InfoBarSeverity.Error;
            SelectionInfoBar.Title = "Catalog failed";
            SelectionInfoBar.Message = ex.Message;
        }
    }

    private void ShowSelectionRequired()
    {
        SelectionInfoBar.Severity = InfoBarSeverity.Warning;
        SelectionInfoBar.Title = "Select a site first";
        SelectionInfoBar.Message = "Choose a site from the list before selecting or simulating.";
    }
}
