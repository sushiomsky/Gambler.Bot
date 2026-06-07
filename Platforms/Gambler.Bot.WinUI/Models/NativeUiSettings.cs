namespace Gambler.Bot.WinUI.Models;

public sealed class NativeUiSettings
{
    public bool UseNativeTheme { get; set; } = true;
    public bool PromptBeforeUpdates { get; set; } = true;
    public string DefaultStorageProvider { get; set; } = "SQLite";
    public bool RiskGuardEnabled { get; set; } = true;
    public bool SessionInsightsEnabled { get; set; } = true;
}
