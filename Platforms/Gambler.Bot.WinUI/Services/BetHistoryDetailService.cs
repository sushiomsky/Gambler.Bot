using System.Globalization;
using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class BetHistoryDetailService : IBetHistoryDetailService
{
    public IReadOnlyList<BetHistoryDetailItem> CreateDetails(BetHistoryRecord record)
    {
        return
        [
            new("Timestamp", record.Timestamp.ToString("O", CultureInfo.InvariantCulture)),
            new("Site", record.Site),
            new("Game", record.Game),
            new("Currency", record.Currency),
            new("Amount", record.Amount.ToString(CultureInfo.InvariantCulture)),
            new("Profit", record.Profit.ToString(CultureInfo.InvariantCulture)),
            new("Outcome", record.Outcome),
            new("Verifier status", record.VerifierStatus),
            new("Server seed", Display(record.ServerSeed)),
            new("Client seed", Display(record.ClientSeed)),
            new("Nonce", record.Nonce?.ToString(CultureInfo.InvariantCulture) ?? "-")
        ];
    }

    private static string Display(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }
}
