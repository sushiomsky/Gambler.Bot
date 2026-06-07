namespace Gambler.Bot.WinUI.Models;

public sealed record BetHistoryRecord(
    DateTimeOffset Timestamp,
    string Site,
    string Game,
    string Currency,
    decimal Amount,
    decimal Profit,
    string Outcome);
