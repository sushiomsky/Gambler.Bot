using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface ISettingsValidationService
{
    NativeUiSettings Normalize(NativeUiSettings settings);
}
