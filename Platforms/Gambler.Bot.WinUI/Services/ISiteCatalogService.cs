using Gambler.Bot.WinUI.Models;
using Gambler.Bot.Core.Sites;

namespace Gambler.Bot.WinUI.Services;

public interface ISiteCatalogService
{
    IReadOnlyList<SiteSummary> GetSites();
    BaseSite? CreateSite(SiteSummary summary);
}
