namespace Gambler.Bot.WinUI.Models;

public sealed record RollVerificationResult(
    bool Succeeded,
    string Message,
    string Site,
    string Game,
    string ServerSeedHash,
    string ResultType,
    string ResultValue);
