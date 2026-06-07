using System.Reflection;
using Gambler.Bot.WinUI.Models;
using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Sources;

namespace Gambler.Bot.WinUI.Services;

public sealed class VelopackUpdateService : IUpdateService
{
    private readonly ILogger<VelopackUpdateService> _logger;

    public VelopackUpdateService(ILogger<VelopackUpdateService> logger)
    {
        _logger = logger;
    }

    public async Task<UpdateStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var manager = new UpdateManager(new GithubSource("https://github.com/Seuntjie900/Gambler.Bot", null, false));
            var updateInfo = await manager.CheckForUpdatesAsync();
            var version = manager.CurrentVersion?.ToFullString() ?? GetAssemblyVersion();

            return new UpdateStatus(
                version,
                manager.IsPortable,
                updateInfo is not null,
                updateInfo is null ? "Up to date" : "Update available");
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex, "Update status is unavailable in the current launch mode.");
            return new UpdateStatus(GetAssemblyVersion(), false, false, "Update status unavailable");
        }
    }

    private static string GetAssemblyVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "0.0.0";
    }
}
