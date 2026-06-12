using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class SettingsPage : Page
{
    private NavigationContext? _navigationContext;
    private NativeUiSettings _settings = new();

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _navigationContext = e.Parameter as NavigationContext;
        if (_navigationContext is null)
        {
            return;
        }

        _settings = await _navigationContext.SettingsService.LoadAsync();
        ApplySettingsToControls();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null)
        {
            return;
        }

        ReadSettingsFromControls();
        await _navigationContext.SettingsService.SaveAsync(_settings);
        SaveInfoBar.IsOpen = true;
    }

    private void ApplySettingsToControls()
    {
        UseNativeThemeToggle.IsOn = _settings.UseNativeTheme;
        PromptBeforeUpdatesToggle.IsOn = _settings.PromptBeforeUpdates;
        RiskGuardToggle.IsOn = _settings.RiskGuardEnabled;
        SessionInsightsToggle.IsOn = _settings.SessionInsightsEnabled;
        DefaultSiteTextBox.Text = _settings.DefaultSite;
        DefaultCurrencyTextBox.Text = _settings.DefaultCurrency;
        DefaultGameTextBox.Text = _settings.DefaultGame;
        EnableAutomationLoopToggle.IsOn = _settings.EnableAutomationLoop;
        AutomationLoopDelayTextBox.Text = _settings.AutomationLoopDelayMs.ToString();
        AutomationMaxSimulationIterationsTextBox.Text = _settings.AutomationMaxSimulationIterations.ToString();
        AllowLiveBetExecutionToggle.IsOn = _settings.AllowLiveBetExecution;
        EnableLiveAutomationLoopToggle.IsOn = _settings.EnableLiveAutomationLoop;
        RequireDecoyCurrencyForLiveBetsToggle.IsOn = _settings.RequireDecoyCurrencyForLiveBets;
        LiveBetConfirmationPhraseTextBox.Text = _settings.LiveBetConfirmationPhrase;
        MinimumBetAmountTextBox.Text = _settings.MinimumBetAmount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        MaximumLiveBetAmountTextBox.Text = _settings.MaximumLiveBetAmount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        MaximumLiveBetsPerRunTextBox.Text = _settings.MaximumLiveBetsPerRun.ToString();
        BetHistoryPageSizeTextBox.Text = _settings.BetHistoryPageSize.ToString();
        ConsoleRetentionEntriesTextBox.Text = _settings.ConsoleRetentionEntries.ToString();
        ChartMaximumPointsTextBox.Text = _settings.ChartMaximumPoints.ToString();

        for (var index = 0; index < StorageProviderComboBox.Items.Count; index++)
        {
            if (StorageProviderComboBox.Items[index] is ComboBoxItem item
                && string.Equals(item.Content?.ToString(), _settings.DefaultStorageProvider, StringComparison.OrdinalIgnoreCase))
            {
                StorageProviderComboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private void ReadSettingsFromControls()
    {
        _settings.UseNativeTheme = UseNativeThemeToggle.IsOn;
        _settings.PromptBeforeUpdates = PromptBeforeUpdatesToggle.IsOn;
        _settings.RiskGuardEnabled = RiskGuardToggle.IsOn;
        _settings.SessionInsightsEnabled = SessionInsightsToggle.IsOn;
        _settings.DefaultSite = DefaultSiteTextBox.Text.Trim();
        _settings.DefaultCurrency = DefaultCurrencyTextBox.Text.Trim();
        _settings.DefaultGame = DefaultGameTextBox.Text.Trim();
        _settings.EnableAutomationLoop = EnableAutomationLoopToggle.IsOn;
        _settings.AutomationLoopDelayMs = ReadInt32(AutomationLoopDelayTextBox.Text, 1000, 100, 60_000);
        _settings.AutomationMaxSimulationIterations = ReadInt32(AutomationMaxSimulationIterationsTextBox.Text, 0, 0, 1_000_000);
        _settings.AllowLiveBetExecution = AllowLiveBetExecutionToggle.IsOn;
        _settings.EnableLiveAutomationLoop = EnableLiveAutomationLoopToggle.IsOn;
        _settings.RequireDecoyCurrencyForLiveBets = RequireDecoyCurrencyForLiveBetsToggle.IsOn;
        _settings.LiveBetConfirmationPhrase = LiveBetConfirmationPhraseTextBox.Text.Trim();
        _settings.MinimumBetAmount = ReadDecimal(MinimumBetAmountTextBox.Text, 0.01m);
        _settings.MaximumLiveBetAmount = ReadDecimal(MaximumLiveBetAmountTextBox.Text, 0.01m);
        _settings.MaximumLiveBetsPerRun = ReadInt32(MaximumLiveBetsPerRunTextBox.Text, 1, 1, 1_000_000);
        _settings.BetHistoryPageSize = ReadInt32(BetHistoryPageSizeTextBox.Text, 250, 25, 10_000);
        _settings.ConsoleRetentionEntries = ReadInt32(ConsoleRetentionEntriesTextBox.Text, 500, 50, 10_000);
        _settings.ChartMaximumPoints = ReadInt32(ChartMaximumPointsTextBox.Text, 120, 10, 5_000);
        _settings.DefaultStorageProvider =
            (StorageProviderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "SQLite";
    }

    private static int ReadInt32(string value, int fallback, int minimum, int maximum)
    {
        return int.TryParse(value, out var parsed)
            ? Math.Clamp(parsed, minimum, maximum)
            : fallback;
    }

    private static decimal ReadDecimal(string value, decimal fallback)
    {
        return decimal.TryParse(value, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }
}
