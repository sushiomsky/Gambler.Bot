using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class ConsolePage : Page
{
    private NavigationContext? _navigationContext;

    public ConsolePage()
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

        _navigationContext.ConsoleLogService.EntryAdded += ConsoleLogService_EntryAdded;
        RefreshEntries();
        if (_navigationContext.ConsoleLogService.Entries.Count == 0)
        {
            _navigationContext.ConsoleLogService.Log("Info", "Console ready. Type 'help' for commands.");
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        if (_navigationContext is not null)
        {
            _navigationContext.ConsoleLogService.EntryAdded -= ConsoleLogService_EntryAdded;
        }
    }

    private void ExecuteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null)
        {
            return;
        }

        var command = CommandTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        _navigationContext.ConsoleLogService.Log("Command", command);
        ExecuteCommand(command);
        CommandTextBox.Text = string.Empty;
    }

    private void SeedButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationContext?.ConsoleLogService.Log("Info", "Manual console marker added.");
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationContext?.ConsoleLogService.Clear();
        RefreshEntries();
    }

    private void ExecuteCommand(string command)
    {
        if (_navigationContext is null)
        {
            return;
        }

        switch (command.ToLowerInvariant())
        {
            case "help":
                _navigationContext.ConsoleLogService.Log("Info", "Commands: help, status, site, strategy, runtime, clear.");
                break;
            case "status":
                _navigationContext.ConsoleLogService.Log("Info", $"{_navigationContext.SiteSessionService.Current.Mode} site mode, {_navigationContext.AutomationStateService.Current.Status} runtime.");
                break;
            case "site":
                _navigationContext.ConsoleLogService.Log("Info", _navigationContext.SiteSessionService.Current.SelectedSite?.Name ?? "No active site.");
                break;
            case "strategy":
                _navigationContext.ConsoleLogService.Log("Info", _navigationContext.StrategySessionService.Current.SelectedStrategy?.Name ?? "No active strategy.");
                break;
            case "runtime":
                var runtime = _navigationContext.AutomationStateService.Current;
                _navigationContext.ConsoleLogService.Log("Info", $"{runtime.Status} / {runtime.Mode} / {runtime.LoopIterations} iterations / {runtime.LastMessage}");
                break;
            case "clear":
                _navigationContext.ConsoleLogService.Clear();
                RefreshEntries();
                break;
            default:
                _navigationContext.ConsoleLogService.Log("Warn", $"Unknown command '{command}'. Type 'help'.");
                break;
        }
    }

    private void ConsoleLogService_EntryAdded(object? sender, Models.ConsoleLogEntry e)
    {
        RefreshEntries();
    }

    private void RefreshEntries()
    {
        if (_navigationContext is null)
        {
            return;
        }

        ConsoleListView.ItemsSource = null;
        ConsoleListView.ItemsSource = _navigationContext.ConsoleLogService.Entries;
    }
}
