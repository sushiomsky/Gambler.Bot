using System.Text.Json;
using Gambler.Bot.WinUI.Models;
using Microsoft.Extensions.Logging;

namespace Gambler.Bot.WinUI.Services;

public sealed class JsonAppSettingsService : IAppSettingsService
{
    private readonly ILogger<JsonAppSettingsService> _logger;
    private readonly ISettingsValidationService _settingsValidationService;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly string? _settingsPath;

    public JsonAppSettingsService(
        ILogger<JsonAppSettingsService> logger,
        ISettingsValidationService? settingsValidationService = null,
        string? settingsPath = null)
    {
        _logger = logger;
        _settingsValidationService = settingsValidationService ?? new SettingsValidationService();
        _settingsPath = settingsPath;
    }

    public async Task<NativeUiSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = GetSettingsPath();
        if (!File.Exists(path))
        {
            return _settingsValidationService.Normalize(new NativeUiSettings());
        }

        try
        {
            await using var stream = File.OpenRead(path);
            var settings = await JsonSerializer.DeserializeAsync<NativeUiSettings>(stream, cancellationToken: cancellationToken)
                ?? new NativeUiSettings();
            return _settingsValidationService.Normalize(settings);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load WinUI settings from {Path}", path);
            return _settingsValidationService.Normalize(new NativeUiSettings());
        }
    }

    public async Task SaveAsync(NativeUiSettings settings, CancellationToken cancellationToken = default)
    {
        var path = GetSettingsPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var normalized = _settingsValidationService.Normalize(settings);

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, normalized, _jsonOptions, cancellationToken);
    }

    private string GetSettingsPath()
    {
        if (!string.IsNullOrWhiteSpace(_settingsPath))
        {
            return _settingsPath;
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "Gambler.Bot", "WinUISettings.json");
    }
}
