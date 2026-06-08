using Gambler.Bot.WinUI.Models;
using Gambler.Bot.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Gambler.Bot.WinUI.Pages;

public sealed partial class StrategiesPage : Page
{
    private NavigationContext? _navigationContext;
    private StrategyScriptDocument? _activeDocument;

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

    private async void OpenEditorButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null || StrategiesListView.SelectedItem is not StrategySummary strategy)
        {
            StrategyInfoBar.Severity = InfoBarSeverity.Warning;
            StrategyInfoBar.Title = "Select a strategy first";
            StrategyInfoBar.Message = "Choose a strategy before opening the editor workspace.";
            return;
        }

        if (!_navigationContext.StrategyScriptService.CanEdit(strategy))
        {
            _activeDocument = null;
            EditorTitleText.Text = "Strategy Editor";
            EditorSubtitleText.Text = $"{strategy.Name} is a preset strategy and is read-only.";
            ScriptPathTextBox.Text = string.Empty;
            ScriptEditorTextBox.Text = string.Empty;
            EditorInfoBar.Severity = InfoBarSeverity.Warning;
            EditorInfoBar.Title = "Read-only strategy";
            EditorInfoBar.Message = "Only Programmer Mode strategies expose editable script documents.";
            return;
        }

        try
        {
            _activeDocument = await _navigationContext.StrategyScriptService.OpenOrCreateAsync(strategy);
            EditorTitleText.Text = $"{strategy.Name} Editor";
            EditorSubtitleText.Text = $"{strategy.Runtime} Programmer Mode script.";
            ScriptPathTextBox.Text = _activeDocument.FilePath;
            ScriptEditorTextBox.Text = _activeDocument.Content;
            EditorInfoBar.Severity = InfoBarSeverity.Success;
            EditorInfoBar.Title = "Script loaded";
            EditorInfoBar.Message = $"Loaded {_activeDocument.FileExtension.ToUpperInvariant()} script from app data.";
        }
        catch (Exception ex)
        {
            EditorInfoBar.Severity = InfoBarSeverity.Error;
            EditorInfoBar.Title = "Editor error";
            EditorInfoBar.Message = ex.Message;
        }
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

    private async void SaveScriptButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null || _activeDocument is null)
        {
            EditorInfoBar.Severity = InfoBarSeverity.Warning;
            EditorInfoBar.Title = "No script loaded";
            EditorInfoBar.Message = "Open a Programmer Mode strategy before saving.";
            return;
        }

        _activeDocument = await _navigationContext.StrategyScriptService.SaveAsync(_activeDocument, ScriptEditorTextBox.Text);
        EditorInfoBar.Severity = InfoBarSeverity.Success;
        EditorInfoBar.Title = "Script saved";
        EditorInfoBar.Message = $"Saved at {_activeDocument.LastSavedAt:HH:mm:ss}.";
    }

    private void ValidateScriptButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationContext is null || _activeDocument is null)
        {
            EditorInfoBar.Severity = InfoBarSeverity.Warning;
            EditorInfoBar.Title = "No script loaded";
            EditorInfoBar.Message = "Open a Programmer Mode strategy before validating.";
            return;
        }

        var document = _activeDocument with { Content = ScriptEditorTextBox.Text };
        var result = _navigationContext.StrategyScriptService.Validate(document);
        EditorInfoBar.Severity = result.Succeeded ? InfoBarSeverity.Success : InfoBarSeverity.Warning;
        EditorInfoBar.Title = result.Succeeded ? "Validation passed" : "Validation warning";
        EditorInfoBar.Message = result.Message;
    }
}
