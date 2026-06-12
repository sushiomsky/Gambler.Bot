namespace Gambler.Bot.WinUI.Models;

public sealed record BetHistoryFilterCriteria(
    string? SearchText = null,
    string? Outcome = null,
    string? Currency = null,
    decimal? MinimumProfit = null,
    decimal? MaximumProfit = null,
    bool VerifierReadyOnly = false);
