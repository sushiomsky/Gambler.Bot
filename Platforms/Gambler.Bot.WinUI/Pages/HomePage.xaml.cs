using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class HomePage : Page
{
    private NavigationContext? _navigationContext;

    public HomePage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not NavigationContext navigationContext)
        {
            return;
        }

        _navigationContext = navigationContext;
        _navigationContext.SiteSessionService.StateChanged += SiteSessionService_StateChanged;
        _navigationContext.StrategySessionService.StateChanged += StrategySessionService_StateChanged;
        _navigationContext.AutomationStateService.StateChanged += AutomationStateService_StateChanged;

        var settingsTask = navigationContext.SettingsService.LoadAsync();
        var updateTask = navigationContext.UpdateService.GetStatusAsync();

        var settings = await settingsTask;
        var updateStatus = await updateTask;

        RiskGuardToggle.IsOn = settings.RiskGuardEnabled;
        SessionInsightsToggle.IsOn = settings.SessionInsightsEnabled;
        StorageInfoBar.Message = $"Default provider: {settings.DefaultStorageProvider}";
        DashboardSubtitleText.Text = updateStatus.HasUpdate
            ? "Update available. Review settings before continuing."
            : "Live session overview, strategy health, and account state.";
        UpdateStatusText.Text = $"{updateStatus.Message}. Version {updateStatus.CurrentVersion}.";
        ApplySessionState(navigationContext.SiteSessionService.Current);
        ApplyStrategyState(navigationContext.StrategySessionService.Current);
        ApplyAutomationState(navigationContext.AutomationStateService.Current);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        if (_navigationContext is not null)
        {
            _navigationContext.SiteSessionService.StateChanged -= SiteSessionService_StateChanged;
            _navigationContext.StrategySessionService.StateChanged -= StrategySessionService_StateChanged;
            _navigationContext.AutomationStateService.StateChanged -= AutomationStateService_StateChanged;
        }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null)
        {
            return;
        }

        var result = _navigationContext.AutomationRuntimeService.Start();
        ShowRuntimeCommandResult(result);
    }

    private void PauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is not null)
        {
            ShowRuntimeCommandResult(_navigationContext.AutomationRuntimeService.Pause());
        }
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is not null)
        {
            ShowRuntimeCommandResult(_navigationContext.AutomationRuntimeService.Stop());
        }
    }

    private void PreviewBetButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null)
        {
            return;
        }

        var result = _navigationContext.BetExecutionService.PrepareNextBet(out var preview);
        BetPreviewInfoBar.IsOpen = true;
        BetPreviewInfoBar.Severity = result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Warning;
        BetPreviewInfoBar.Message = preview is null
            ? result.Message
            : $"{preview.Site} / {preview.Strategy}: {preview.Amount:0.########} {preview.Currency} on {preview.Game} ({preview.Details})";
    }

    private async void LiveBetButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null)
        {
            return;
        }

        var result = await _navigationContext.BetExecutionService.ExecuteLiveBetAsync();
        BetPreviewInfoBar.IsOpen = true;
        BetPreviewInfoBar.Severity = result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Warning;
        BetPreviewInfoBar.Message = result.Message;
        _navigationContext.ConsoleLogService.Log(result.Succeeded ? "LiveBet" : "Warn", result.Message);
    }

    private void SiteSessionService_StateChanged(object? sender, Models.SiteSessionState e)
    {
        ApplySessionState(e);
    }

    private void ApplySessionState(Models.SiteSessionState state)
    {
        if (state.SelectedSite is null)
        {
            SessionInfoBar.Severity = InfoBarSeverity.Informational;
            SessionInfoBar.Message = "No live session connected.";
            BalanceCaptionText.Text = "Waiting for site login";
            return;
        }

        SessionInfoBar.Severity = state.IsConnected ? InfoBarSeverity.Success : InfoBarSeverity.Informational;
        SessionInfoBar.Message = $"{state.SelectedSite.Name} active in {state.Mode.ToLowerInvariant()} mode.";
        BalanceCaptionText.Text = $"{state.SelectedSite.Name} context active";
        ActivityListView.Items.Insert(0, new ListViewItem
        {
            Content = $"{state.SelectedSite.Name} selected for {state.Mode.ToLowerInvariant()}."
        });
    }

    private void StrategySessionService_StateChanged(object? sender, Models.StrategySessionState e)
    {
        ApplyStrategyState(e);
    }

    private void ApplyStrategyState(Models.StrategySessionState state)
    {
        if (state.SelectedStrategy is null)
        {
            StrategyInfoBar.Severity = InfoBarSeverity.Informational;
            StrategyInfoBar.Message = "No strategy selected.";
            return;
        }

        StrategyInfoBar.Severity = InfoBarSeverity.Success;
        StrategyInfoBar.Message = $"{state.SelectedStrategy.Name} selected ({state.SelectedStrategy.Runtime}).";
        ActivityListView.Items.Insert(0, new ListViewItem
        {
            Content = $"{state.SelectedStrategy.Name} selected as active strategy."
        });
    }

    private void AutomationStateService_StateChanged(object? sender, Models.AutomationState e)
    {
        ApplyAutomationState(e);
    }

    private void ApplyAutomationState(Models.AutomationState state)
    {
        RuntimeInfoBar.Severity = state.Status == "Running" ? InfoBarSeverity.Success : InfoBarSeverity.Informational;
        RuntimeInfoBar.Message = state.StartedAt is null
            ? state.Status
            : $"{state.Status} since {state.StartedAt:t} ({state.Mode}, {state.LoopIterations} iterations)";
        LoopStatusText.Text = state.LastMessage;
        LoopIterationsText.Text = state.LoopIterations.ToString("N0");

        if (state.LastBetPreview is not null)
        {
            ActivityListView.Items.Insert(0, new ListViewItem
            {
                Content = $"{state.LastBetPreview.Site} / {state.LastBetPreview.Strategy}: {state.LastBetPreview.Amount:0.########} {state.LastBetPreview.Currency} on {state.LastBetPreview.Game}."
            });
        }
    }

    private void ShowRuntimeCommandResult(Models.AutomationCommandResult result)
    {
        RuntimeInfoBar.Severity = result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Warning;
        RuntimeInfoBar.Message = result.Message;
    }
}
