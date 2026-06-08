namespace Gambler.Bot.WinUI.Models;

public sealed record BetHistorySummary(
    int TotalRecords,
    int Wins,
    int Losses,
    decimal TotalAmount,
    decimal NetProfit)
{
    public decimal WinRate => TotalRecords == 0 ? 0m : Math.Round(Wins / (decimal)TotalRecords * 100m, 2);
}
