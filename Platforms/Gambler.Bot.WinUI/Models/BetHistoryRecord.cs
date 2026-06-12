namespace Gambler.Bot.WinUI.Models;

public sealed record BetHistoryRecord(
    DateTimeOffset Timestamp,
    string Site,
    string Game,
    string Currency,
    decimal Amount,
    decimal Profit,
    string Outcome,
    string? ServerSeed = null,
    string? ClientSeed = null,
    long? Nonce = null)
{
    public bool CanPrefillVerifier =>
        !string.IsNullOrWhiteSpace(ServerSeed)
        && !string.IsNullOrWhiteSpace(ClientSeed)
        && Nonce.HasValue;

    public string VerifierStatus => CanPrefillVerifier ? "Fair" : "No seed";
}
