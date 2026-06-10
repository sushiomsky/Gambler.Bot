namespace Gambler.Bot.WinUI.Models;

public sealed record RollVerificationRequest(
    SiteSummary Site,
    string Game,
    string ServerSeed,
    string ClientSeed,
    int Nonce);
