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
        EnableAutomationLoopToggle.IsOn = _settings.EnableAutomationLoop;
        AutomationLoopDelayTextBox.Text = _settings.AutomationLoopDelayMs.ToString();
        AutomationMaxSimulationIterationsTextBox.Text = _settings.AutomationMaxSimulationIterations.ToString();
        AllowLiveBetExecutionToggle.IsOn = _settings.AllowLiveBetExecution;
        LiveBetConfirmationPhraseTextBox.Text = _settings.LiveBetConfirmationPhrase;

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
        _settings.EnableAutomationLoop = EnableAutomationLoopToggle.IsOn;
        _settings.AutomationLoopDelayMs = ReadInt32(AutomationLoopDelayTextBox.Text, 1000, 100, 60_000);
        _settings.AutomationMaxSimulationIterations = ReadInt32(AutomationMaxSimulationIterationsTextBox.Text, 0, 0, 1_000_000);
        _settings.AllowLiveBetExecution = AllowLiveBetExecutionToggle.IsOn;
        _settings.LiveBetConfirmationPhrase = LiveBetConfirmationPhraseTextBox.Text.Trim();
        _settings.DefaultStorageProvider =
            (StorageProviderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "SQLite";
    }

    private static int ReadInt32(string value, int fallback, int minimum, int maximum)
    {
        return int.TryParse(value, out var parsed)
            ? Math.Clamp(parsed, minimum, maximum)
            : fallback;
    }
}
