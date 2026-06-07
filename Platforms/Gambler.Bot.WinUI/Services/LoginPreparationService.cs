using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class LoginPreparationService : ILoginPreparationService
{
    private readonly ISiteCatalogService _siteCatalogService;

    public LoginPreparationService(ISiteCatalogService siteCatalogService)
    {
        _siteCatalogService = siteCatalogService;
    }

    public LoginProfile? GetProfile(SiteSummary site)
    {
        var siteInstance = _siteCatalogService.CreateSite(site);
        if (siteInstance is null)
        {
            return null;
        }

        var fields = siteInstance.LoginParams
            .Select(parameter => new LoginFieldModel
            {
                Name = parameter.Name,
                IsRequired = parameter.Required,
                IsSecret = parameter.Masked,
                IsMfa = parameter.IsMFA
            })
            .ToList();

        return new LoginProfile(
            site,
            siteInstance.SupportsNormalLogin,
            siteInstance.SupportsBrowserLogin,
            siteInstance.Mirrors.Count == 0 ? [siteInstance.SiteURL] : siteInstance.Mirrors,
            fields);
    }

    public AutomationCommandResult ValidateFields(LoginProfile profile)
    {
        var missing = profile.Fields
            .Where(field => field.IsRequired && string.IsNullOrWhiteSpace(field.Value))
            .Select(field => field.Name)
            .ToArray();

        return missing.Length == 0
            ? new AutomationCommandResult(true, "Login fields are complete.")
            : new AutomationCommandResult(false, $"Required fields missing: {string.Join(", ", missing)}.");
    }
}
