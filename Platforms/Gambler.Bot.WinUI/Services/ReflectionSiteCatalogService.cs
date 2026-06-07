using System.Reflection;
using Gambler.Bot.Core.Sites;
using Gambler.Bot.WinUI.Models;
using Microsoft.Extensions.Logging;

namespace Gambler.Bot.WinUI.Services;

public sealed class ReflectionSiteCatalogService : ISiteCatalogService
{
    private readonly ILogger<ReflectionSiteCatalogService> _logger;
    private IReadOnlyList<SiteSummary>? _sites;

    public ReflectionSiteCatalogService(ILogger<ReflectionSiteCatalogService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<SiteSummary> GetSites()
    {
        if (_sites is not null)
        {
            return _sites;
        }

        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        var siteLogger = loggerFactory.CreateLogger<BaseSite>();
        var baseType = typeof(BaseSite);
        var siteTypes = Assembly.GetAssembly(baseType)?.GetTypes() ?? [];

        _sites = siteTypes
            .Where(type => type.IsSubclassOf(baseType) && !type.IsAbstract)
            .Select(type => CreateSummary(type, siteLogger))
            .Where(summary => summary is not null)
            .Cast<SiteSummary>()
            .OrderBy(summary => summary.Name)
            .ToList();

        return _sites;
    }

    public BaseSite? CreateSite(SiteSummary summary)
    {
        var siteType = Assembly.GetAssembly(typeof(BaseSite))?
            .GetType(summary.SiteTypeName, throwOnError: false);

        if (siteType is null)
        {
            return null;
        }

        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        return Activator.CreateInstance(siteType, loggerFactory.CreateLogger<BaseSite>()) as BaseSite;
    }

    private SiteSummary? CreateSummary(Type siteType, ILogger siteLogger)
    {
        try
        {
            if (Activator.CreateInstance(siteType, siteLogger) is not BaseSite site || !site.IsEnabled)
            {
                return null;
            }

            return new SiteSummary(
                site.SiteName,
                site.SiteURL,
                site.Currencies?.Select(currency => currency.ToUpperInvariant()).ToArray() ?? [],
                site.SupportedGames?.Select(game => game.ToString()).ToArray() ?? [],
                "Available",
                siteType.FullName ?? siteType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load site metadata for {SiteType}", siteType.Name);
            return null;
        }
    }
}
