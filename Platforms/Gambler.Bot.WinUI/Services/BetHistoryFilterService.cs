using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class BetHistoryFilterService : IBetHistoryFilterService
{
    public IReadOnlyList<BetHistoryRecord> Apply(
        IReadOnlyList<BetHistoryRecord> records,
        string? searchText,
        string? outcome)
    {
        var query = records.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(outcome) && !string.Equals(outcome, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(record => string.Equals(record.Outcome, outcome, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(record => MatchesSearch(record, searchText));
        }

        return query.ToList();
    }

    private static bool MatchesSearch(BetHistoryRecord record, string searchText)
    {
        return Contains(record.Site, searchText)
            || Contains(record.Game, searchText)
            || Contains(record.Currency, searchText)
            || Contains(record.Outcome, searchText)
            || Contains(record.ServerSeed, searchText)
            || Contains(record.ClientSeed, searchText)
            || Contains(record.Nonce?.ToString(), searchText);
    }

    private static bool Contains(string? value, string searchText)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}
