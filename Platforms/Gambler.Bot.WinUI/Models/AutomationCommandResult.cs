namespace Gambler.Bot.WinUI.Models;

public sealed record AutomationCommandResult(
    bool Succeeded,
    string Message,
    decimal Profit = 0);
