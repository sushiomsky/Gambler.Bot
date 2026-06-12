namespace Gambler.Bot.WinUI.Models;

public sealed record BetChartSnapshot(
    string Sparkline,
    decimal StartProfit,
    decimal EndProfit,
    decimal BestProfit,
    decimal WorstProfit,
    int Wins,
    int Losses,
    decimal AverageProfit,
    decimal ReturnOnInvestmentPercent,
    decimal MaximumDrawdown,
    int LongestWinStreak,
    int LongestLossStreak);
