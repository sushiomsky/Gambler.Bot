namespace Gambler.Bot.WinUI.Models;

public sealed record LoginProfile(
    SiteSummary Site,
    bool SupportsNormalLogin,
    bool SupportsBrowserLogin,
    IReadOnlyList<string> Mirrors,
    IReadOnlyList<string> CurrencyChoices,
    IReadOnlyList<LoginFieldModel> Fields)
{
    public string SelectedCurrency { get; set; } = CurrencyChoices.FirstOrDefault() ?? Site.Currencies.FirstOrDefault() ?? string.Empty;
}
