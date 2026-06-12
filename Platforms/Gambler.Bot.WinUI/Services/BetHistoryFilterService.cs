using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class BetHistoryFilterService : IBetHistoryFilterService
{
    public IReadOnlyList<BetHistoryRecord> Apply(
        IReadOnlyList<BetHistoryRecord> records,
        string? searchText,
        string? outcome)
    {
        return Apply(records, new BetHistoryFilterCriteria(searchText, outcome));
    }

    public IReadOnlyList<BetHistoryRecord> Apply(
        IReadOnlyList<BetHistoryRecord> records,
        BetHistoryFilterCriteria criteria)
    {
        var query = records.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(criteria.Outcome) && !string.Equals(criteria.Outcome, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(record => string.Equals(record.Outcome, criteria.Outcome, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Currency))
        {
            query = query.Where(record => string.Equals(record.Currency, criteria.Currency.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.MinimumProfit.HasValue)
        {
            query = query.Where(record => record.Profit >= criteria.MinimumProfit.Value);
        }

        if (criteria.MaximumProfit.HasValue)
        {
            query = query.Where(record => record.Profit <= criteria.MaximumProfit.Value);
        }

        if (criteria.VerifierReadyOnly)
        {
            query = query.Where(record => record.CanPrefillVerifier);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            query = query.Where(record => MatchesSearch(record, criteria.SearchText));
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
