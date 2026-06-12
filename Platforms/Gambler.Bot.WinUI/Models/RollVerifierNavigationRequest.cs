using Gambler.Bot.WinUI.Services;

namespace Gambler.Bot.WinUI.Models;

public sealed record RollVerifierNavigationRequest(
    NavigationContext NavigationContext,
    BetHistoryRecord? SourceBet);
