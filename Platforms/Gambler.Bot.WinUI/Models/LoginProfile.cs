namespace Gambler.Bot.WinUI.Models;

public sealed record LoginProfile(
    SiteSummary Site,
    bool SupportsNormalLogin,
    bool SupportsBrowserLogin,
    IReadOnlyList<string> Mirrors,
    IReadOnlyList<LoginFieldModel> Fields);
