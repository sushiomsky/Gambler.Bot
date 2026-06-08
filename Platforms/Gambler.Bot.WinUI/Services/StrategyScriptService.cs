using System.Text.RegularExpressions;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.WinUI.Models;

namespace Gambler.Bot.WinUI.Services;

public sealed class StrategyScriptService : IStrategyScriptService
{
    private static readonly Regex UnsafeFileNameCharacters = new(@"[^a-zA-Z0-9._-]+", RegexOptions.Compiled);
    private readonly IStrategyCatalogService _strategyCatalogService;
    private readonly string? _storageRoot;

    public StrategyScriptService(IStrategyCatalogService strategyCatalogService, string? storageRoot = null)
    {
        _strategyCatalogService = strategyCatalogService;
        _storageRoot = storageRoot;
    }

    public bool CanEdit(StrategySummary strategy)
    {
        return string.Equals(strategy.Kind, "Programmer Mode", StringComparison.OrdinalIgnoreCase);
    }

    public string GetDefaultScriptPath(StrategySummary strategy)
    {
        var directory = _storageRoot
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gambler.Bot", "scripts");
        return Path.Combine(directory, $"{Sanitize(strategy.Name)}.{GetFileExtension(strategy)}");
    }

    public async Task<StrategyScriptDocument> OpenOrCreateAsync(StrategySummary strategy, CancellationToken cancellationToken = default)
    {
        if (!CanEdit(strategy))
        {
            throw new InvalidOperationException($"{strategy.Name} is a preset strategy and does not expose a script document.");
        }

        var filePath = GetDefaultScriptPath(strategy);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        if (!File.Exists(filePath))
        {
            await File.WriteAllTextAsync(filePath, CreateTemplate(strategy), cancellationToken);
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        ApplyFileName(strategy, filePath);
        return new StrategyScriptDocument(
            strategy,
            strategy.Runtime,
            GetFileExtension(strategy),
            filePath,
            content,
            File.GetLastWriteTimeUtc(filePath));
    }

    public async Task<StrategyScriptDocument> SaveAsync(StrategyScriptDocument document, string content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(document.FilePath)!);
        await File.WriteAllTextAsync(document.FilePath, content, cancellationToken);
        ApplyFileName(document.Strategy, document.FilePath);

        return document with
        {
            Content = content,
            LastSavedAt = File.GetLastWriteTimeUtc(document.FilePath)
        };
    }

    public AutomationCommandResult Validate(StrategyScriptDocument document)
    {
        if (string.IsNullOrWhiteSpace(document.Content))
        {
            return new AutomationCommandResult(false, "Script is empty.");
        }

        var requiredEntryPoint = document.Runtime switch
        {
            "C#" => "DoDiceBet",
            "JavaScript" => "dobet",
            "Lua" => "dobet",
            "Python" => "dobet",
            _ => string.Empty
        };

        if (!string.IsNullOrWhiteSpace(requiredEntryPoint)
            && !document.Content.Contains(requiredEntryPoint, StringComparison.OrdinalIgnoreCase))
        {
            return new AutomationCommandResult(false, $"Script should define the {requiredEntryPoint} entry point.");
        }

        return new AutomationCommandResult(true, "Script passed basic validation.");
    }

    private void ApplyFileName(StrategySummary strategy, string filePath)
    {
        var runtime = _strategyCatalogService.CreateStrategy(strategy);
        if (runtime is IProgrammerMode programmerMode)
        {
            programmerMode.FileName = filePath;
        }
    }

    private static string GetFileExtension(StrategySummary strategy)
    {
        return strategy.Runtime switch
        {
            "C#" => "csx",
            "JavaScript" => "js",
            "Lua" => "lua",
            "Python" => "py",
            _ => "txt"
        };
    }

    private static string Sanitize(string value)
    {
        return UnsafeFileNameCharacters.Replace(value.Trim(), "-").Trim('-').ToLowerInvariant();
    }

    private static string CreateTemplate(StrategySummary strategy)
    {
        return strategy.Runtime switch
        {
            "C#" => """
                // C# Programmer Mode template
                // Implement DoDiceBet(previousBet, win, nextBet) and ResetDice(nextBet).

                void ResetDice(dynamic nextBet)
                {
                    nextBet.Amount = 0.01m;
                    nextBet.Chance = 49.5m;
                    nextBet.High = true;
                }

                void DoDiceBet(dynamic previousBet, bool win, dynamic nextBet)
                {
                    nextBet.Amount = 0.01m;
                    nextBet.Chance = 49.5m;
                    nextBet.High = !win;
                }
                """,
            "JavaScript" => """
                // JavaScript Programmer Mode template
                function dobet(previousBet, win, nextBet) {
                    nextBet.Amount = 0.01;
                    nextBet.Chance = 49.5;
                    nextBet.High = !win;
                }
                """,
            "Python" => """
                # Python Programmer Mode template
                def dobet(previous_bet, win, next_bet):
                    next_bet.Amount = 0.01
                    next_bet.Chance = 49.5
                    next_bet.High = not win
                """,
            _ => """
                -- Lua Programmer Mode template
                function dobet(previousBet, win, nextBet)
                    nextBet.Amount = 0.01
                    nextBet.Chance = 49.5
                    nextBet.High = not win
                end
                """
        };
    }
}
