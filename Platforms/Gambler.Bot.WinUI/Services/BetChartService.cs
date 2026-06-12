using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class BetChartService : IBetChartService
{
    private static readonly char[] SparklineTicks = ['_', '▁', '▂', '▃', '▄', '▅', '▆', '▇', '█'];

    public BetChartSnapshot CreateSnapshot(IReadOnlyList<BetHistoryRecord> records)
    {
        if (records.Count == 0)
        {
            return new BetChartSnapshot("No data", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        var ordered = records.OrderBy(record => record.Timestamp).ToList();
        var cumulative = new List<decimal>(ordered.Count);
        decimal running = 0;
        decimal peak = 0;
        decimal maximumDrawdown = 0;

        foreach (var record in ordered)
        {
            running += record.Profit;
            cumulative.Add(running);
            peak = Math.Max(peak, running);
            maximumDrawdown = Math.Max(maximumDrawdown, peak - running);
        }

        var totalWagered = ordered.Sum(record => record.Amount);
        return new BetChartSnapshot(
            CreateSparkline(cumulative),
            cumulative.First(),
            cumulative.Last(),
            cumulative.Max(),
            cumulative.Min(),
            ordered.Count(IsWin),
            ordered.Count(record => !IsWin(record)),
            ordered.Average(record => record.Profit),
            totalWagered == 0 ? 0 : cumulative.Last() / totalWagered * 100m,
            maximumDrawdown,
            LongestStreak(ordered, win: true),
            LongestStreak(ordered, win: false));
    }

    private static string CreateSparkline(IReadOnlyList<decimal> values)
    {
        var minimum = values.Min();
        var maximum = values.Max();
        if (minimum == maximum)
        {
            return new string('▄', values.Count);
        }

        var range = maximum - minimum;
        return string.Concat(values.Select(value =>
        {
            var normalized = (value - minimum) / range;
            var index = (int)Math.Round(normalized * (SparklineTicks.Length - 1), MidpointRounding.AwayFromZero);
            return SparklineTicks[Math.Clamp(index, 0, SparklineTicks.Length - 1)];
        }));
    }

    private static bool IsWin(BetHistoryRecord record)
    {
        return string.Equals(record.Outcome, "Win", StringComparison.OrdinalIgnoreCase);
    }

    private static int LongestStreak(IEnumerable<BetHistoryRecord> records, bool win)
    {
        var longest = 0;
        var current = 0;

        foreach (var record in records)
        {
            if (IsWin(record) == win)
            {
                current++;
                longest = Math.Max(longest, current);
            }
            else
            {
                current = 0;
            }
        }

        return longest;
    }
}
