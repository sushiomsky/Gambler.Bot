namespace Gambler.Bot.WinUI.Models;

public sealed class NativeUiSettings
{
    public bool UseNativeTheme { get; set; } = true;
    public bool PromptBeforeUpdates { get; set; } = true;
    public string DefaultStorageProvider { get; set; } = "SQLite";
    public string DefaultSite { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "DECOY";
    public string DefaultGame { get; set; } = "Dice";
    public bool RiskGuardEnabled { get; set; } = true;
    public bool SessionInsightsEnabled { get; set; } = true;
    public bool EnableAutomationLoop { get; set; } = true;
    public bool EnableLiveAutomationLoop { get; set; } = false;
    public int AutomationLoopDelayMs { get; set; } = 1000;
    public int AutomationMaxSimulationIterations { get; set; } = 0;
    public decimal MinimumBetAmount { get; set; } = 0.01m;
    public decimal MaximumLiveBetAmount { get; set; } = 0.01m;
    public int MaximumLiveBetsPerRun { get; set; } = 1;
    public decimal LiveStopLossAmount { get; set; } = 0.05m;
    public decimal LiveTakeProfitAmount { get; set; } = 0.05m;
    public bool RequireDecoyCurrencyForLiveBets { get; set; } = true;
    public bool AllowLiveBetExecution { get; set; } = false;
    public string LiveBetConfirmationPhrase { get; set; } = string.Empty;
    public int BetHistoryPageSize { get; set; } = 250;
    public int ConsoleRetentionEntries { get; set; } = 500;
    public int ChartMaximumPoints { get; set; } = 120;
}
