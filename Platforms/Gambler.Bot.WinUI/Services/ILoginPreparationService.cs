using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface ILoginPreparationService
{
    LoginProfile? GetProfile(SiteSummary site);
    AutomationCommandResult ValidateFields(LoginProfile profile);
}
