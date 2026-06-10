using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public interface IRollVerifierService
{
    RollVerificationResult Verify(RollVerificationRequest request);
}
