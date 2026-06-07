using System.Text.Json;
using Gambler.Bot.WinUI.Models;
using Microsoft.Extensions.Logging;

namespace Gambler.Bot.WinUI.Services;

public sealed class JsonAppSettingsService : IAppSettingsService
{
    private readonly ILogger<JsonAppSettingsService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public JsonAppSettingsService(ILogger<JsonAppSettingsService> logger)
    {
        _logger = logger;
    }

    public async Task<NativeUiSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = GetSettingsPath();
        if (!File.Exists(path))
        {
            return new NativeUiSettings();
        }

        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<NativeUiSettings>(stream, cancellationToken: cancellationToken)
                ?? new NativeUiSettings();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load WinUI settings from {Path}", path);
            return new NativeUiSettings();
        }
    }

    public async Task SaveAsync(NativeUiSettings settings, CancellationToken cancellationToken = default)
    {
        var path = GetSettingsPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions, cancellationToken);
    }

    private static string GetSettingsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "Gambler.Bot", "WinUISettings.json");
    }
}
