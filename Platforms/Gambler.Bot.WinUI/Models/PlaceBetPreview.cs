namespace Gambler.Bot.WinUI.Models;

public sealed record PlaceBetPreview(
    string Site,
    string Strategy,
    string Game,
    string Currency,
    decimal Amount,
    string Details);
