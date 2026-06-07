using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class StrategiesPage : Page
{
    private NavigationContext? _navigationContext;

    public StrategiesPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _navigationContext = e.Parameter as NavigationContext;
        LoadStrategies();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        LoadStrategies();
    }

    private void OpenEditorButton_Click(object sender, RoutedEventArgs e)
    {
        if (StrategiesListView.SelectedItem is not StrategySummary strategy)
        {
            StrategyInfoBar.Severity = InfoBarSeverity.Warning;
            StrategyInfoBar.Title = "Select a strategy first";
            StrategyInfoBar.Message = "Choose a strategy before opening the editor workspace.";
            return;
        }

        StrategyInfoBar.Severity = InfoBarSeverity.Informational;
        StrategyInfoBar.Title = "Editor pending";
        StrategyInfoBar.Message = $"{strategy.Name} is ready for the native editor migration step.";
    }

    private void SelectButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null || StrategiesListView.SelectedItem is not StrategySummary strategy)
        {
            StrategyInfoBar.Severity = InfoBarSeverity.Warning;
            StrategyInfoBar.Title = "Select a strategy first";
            StrategyInfoBar.Message = "Choose a strategy before making it active.";
            return;
        }

        _navigationContext.StrategySessionService.Select(strategy);
        StrategyInfoBar.Severity = InfoBarSeverity.Success;
        StrategyInfoBar.Title = "Strategy selected";
        StrategyInfoBar.Message = $"{strategy.Name} is now the active strategy context.";
    }

    private void LoadStrategies()
    {
        if (_navigationContext is null)
        {
            return;
        }

        var strategies = _navigationContext.StrategyCatalogService.GetStrategies();
        StrategiesListView.ItemsSource = strategies;
        StrategyCountText.Text = $"{strategies.Count} strategies discovered from Gambler.Bot.Strategies.";
    }
}
