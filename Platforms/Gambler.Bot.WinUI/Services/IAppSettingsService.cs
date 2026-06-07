using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IAppSettingsService
{
    Task<NativeUiSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(NativeUiSettings settings, CancellationToken cancellationToken = default);
}
