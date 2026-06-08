namespace Gambler.Bot.WinUI.Models;

public sealed class NativeUiSettings
{
    public bool UseNativeTheme { get; set; } = true;
    public bool PromptBeforeUpdates { get; set; } = true;
    public string DefaultStorageProvider { get; set; } = "SQLite";
    public bool RiskGuardEnabled { get; set; } = true;
    public bool SessionInsightsEnabled { get; set; } = true;
    public bool EnableAutomationLoop { get; set; } = true;
    public int AutomationLoopDelayMs { get; set; } = 1000;
    public int AutomationMaxSimulationIterations { get; set; } = 0;
    public bool AllowLiveBetExecution { get; set; } = false;
    public string LiveBetConfirmationPhrase { get; set; } = string.Empty;
}
