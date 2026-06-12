using Gambler.Bot.Core.Sites.Classes;
using Gambler.Bot.WinUI.Models;
using Microsoft.Extensions.Logging;

namespace Gambler.Bot.WinUI.Services;

public sealed class LiveLoginService : ILiveLoginService
{
    private readonly ISiteCatalogService _siteCatalogService;
    private readonly ISiteSessionService _siteSessionService;
    private readonly IAppSettingsService _settingsService;
    private readonly ILogger<LiveLoginService> _logger;

    public LiveLoginService(
        ISiteCatalogService siteCatalogService,
        ISiteSessionService siteSessionService,
        IAppSettingsService settingsService,
        ILogger<LiveLoginService> logger)
    {
        _siteCatalogService = siteCatalogService;
        _siteSessionService = siteSessionService;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task<AutomationCommandResult> LoginAsync(LoginProfile profile, CancellationToken cancellationToken = default)
    {
        var site = _siteCatalogService.CreateSite(profile.Site);
        if (site is null)
        {
            ClearSensitiveValues(profile);
            return new AutomationCommandResult(false, $"{profile.Site.Name} could not be created from Core.");
        }

        if (!profile.SupportsNormalLogin)
        {
            ClearSensitiveValues(profile);
            return new AutomationCommandResult(false, $"{profile.Site.Name} does not support normal login.");
        }

        var missing = profile.Fields
            .Where(field => field.IsRequired && string.IsNullOrWhiteSpace(field.Value))
            .Select(field => field.Name)
            .ToArray();

        if (missing.Length > 0)
        {
            return new AutomationCommandResult(false, $"Required fields missing: {string.Join(", ", missing)}.");
        }

        try
        {
            var settings = await _settingsService.LoadAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(settings.DefaultCurrency))
            {
                site.CurrentCurrency = settings.DefaultCurrency;
            }

            var parameters = site.LoginParams
                .Select((parameter, index) => new LoginParamValue
                {
                    ParameterId = index,
                    Param = parameter,
                    Value = profile.Fields.ElementAtOrDefault(index)?.Value
                })
                .ToArray();

            var url = profile.Mirrors.FirstOrDefault() ?? site.SiteURL;
            var success = await site.LogIn(url, parameters);

            if (success)
            {
                _siteSessionService.SetLiveConnected(profile.Site, site);
                return new AutomationCommandResult(true, $"{profile.Site.Name} login succeeded.");
            }

            return new AutomationCommandResult(false, $"{profile.Site.Name} login failed.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Live login failed for {SiteName}", profile.Site.Name);
            return new AutomationCommandResult(false, $"{profile.Site.Name} login failed: {ex.Message}");
        }
        finally
        {
            ClearSensitiveValues(profile);
        }
    }

    private static void ClearSensitiveValues(LoginProfile profile)
    {
        foreach (var field in profile.Fields.Where(field => field.IsSecret || field.IsMfa))
        {
            field.Value = null;
        }
    }
}
