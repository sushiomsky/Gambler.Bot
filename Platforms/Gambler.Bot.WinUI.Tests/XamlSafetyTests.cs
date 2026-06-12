using Xunit;

namespace Gambler.Bot.WinUI.Tests;

public sealed class XamlSafetyTests
{
    [Fact]
    public void PagesDoNotUseInfoBadgeForTextStatusPills()
    {
        var pagesDirectory = FindPagesDirectory();
        var xamlFiles = Directory.GetFiles(pagesDirectory, "*.xaml", SearchOption.TopDirectoryOnly);

        var offenders = xamlFiles
            .Where(file => File.ReadAllText(file).Contains("<InfoBadge", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName)
            .ToArray();

        Assert.Empty(offenders);
    }

    private static string FindPagesDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "Platforms", "Gambler.Bot.WinUI", "Pages");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Platforms/Gambler.Bot.WinUI/Pages.");
    }
}
