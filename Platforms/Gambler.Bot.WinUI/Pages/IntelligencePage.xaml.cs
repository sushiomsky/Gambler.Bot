using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class IntelligencePage : Page
{
    private NavigationContext? _navigationContext;

    public IntelligencePage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _navigationContext = e.Parameter as NavigationContext;
        await LoadInsightsAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadInsightsAsync();
    }

    private async Task LoadInsightsAsync()
    {
        if (_navigationContext is null)
        {
            return;
        }

        var insights = await _navigationContext.InsightService.GetInsightsAsync();
        InsightListView.ItemsSource = insights;
        InsightSubtitleText.Text = $"{insights.Count} diagnostics loaded.";
    }
}
