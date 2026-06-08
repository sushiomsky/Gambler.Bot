using Gambler.Bot.WinUI.Services;
using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class VelopackUpdateServiceTests
{
    [Fact]
    public void DefaultUpdateUrlUsesProjectRepository()
    {
        Assert.Equal("https://github.com/sushiomsky/Gambler.Bot", VelopackUpdateService.DefaultUpdateUrl);
    }
}
