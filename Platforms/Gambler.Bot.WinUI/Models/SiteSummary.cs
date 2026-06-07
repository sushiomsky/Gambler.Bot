namespace Gambler.Bot.WinUI.Models;

public sealed record SiteSummary(
    string Name,
    string Url,
    IReadOnlyList<string> Currencies,
    IReadOnlyList<string> Games,
    string Status,
    string SiteTypeName)
{
    public string CurrencyText => string.Join(", ", Currencies);
    public string GameText => string.Join(", ", Games);
}
