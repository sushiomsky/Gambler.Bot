using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class SettingsValidationService : ISettingsValidationService
{
    public NativeUiSettings Normalize(NativeUiSettings settings)
    {
        settings.DefaultStorageProvider = NormalizeText(settings.DefaultStorageProvider, "SQLite");
        settings.DefaultSite = settings.DefaultSite.Trim();
        settings.DefaultCurrency = NormalizeText(settings.DefaultCurrency, "DECOY").ToUpperInvariant();
        settings.DefaultGame = NormalizeText(settings.DefaultGame, "Dice");
        settings.AutomationLoopDelayMs = Math.Clamp(settings.AutomationLoopDelayMs, 100, 60_000);
        settings.AutomationMaxSimulationIterations = Math.Clamp(settings.AutomationMaxSimulationIterations, 0, 1_000_000);
        settings.MinimumBetAmount = Clamp(settings.MinimumBetAmount, 0.00000001m, 1_000_000m);
        settings.MaximumLiveBetAmount = Clamp(settings.MaximumLiveBetAmount, settings.MinimumBetAmount, 1_000_000m);
        settings.MaximumLiveBetsPerRun = Math.Clamp(settings.MaximumLiveBetsPerRun, 1, 1_000_000);
        settings.LiveStopLossAmount = Clamp(settings.LiveStopLossAmount, 0.00000001m, 1_000_000m);
        settings.LiveTakeProfitAmount = Clamp(settings.LiveTakeProfitAmount, 0.00000001m, 1_000_000m);
        settings.BetHistoryPageSize = Math.Clamp(settings.BetHistoryPageSize, 25, 10_000);
        settings.ConsoleRetentionEntries = Math.Clamp(settings.ConsoleRetentionEntries, 50, 10_000);
        settings.ChartMaximumPoints = Math.Clamp(settings.ChartMaximumPoints, 10, 5_000);
        settings.LiveBetConfirmationPhrase = settings.LiveBetConfirmationPhrase.Trim();
        return settings;
    }

    private static string NormalizeText(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static decimal Clamp(decimal value, decimal minimum, decimal maximum)
    {
        if (value < minimum)
        {
            return minimum;
        }

        if (value > maximum)
        {
            return maximum;
        }

        return value;
    }
}
