namespace Gambler.Bot.WinUI.Models;

public sealed record UpdateStatus(
    string CurrentVersion,
    bool IsPortable,
    bool HasUpdate,
    string Message);
