using System.Reflection;
using Gambler.Bot.Strategies.Strategies.Abstractions;
using Gambler.Bot.WinUI.Models;
using Microsoft.Extensions.Logging;

namespace Gambler.Bot.WinUI.Services;

public sealed class ReflectionStrategyCatalogService : IStrategyCatalogService
{
    private readonly ILogger<ReflectionStrategyCatalogService> _logger;
    private IReadOnlyList<StrategySummary>? _strategies;

    public ReflectionStrategyCatalogService(ILogger<ReflectionStrategyCatalogService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<StrategySummary> GetStrategies()
    {
        if (_strategies is not null)
        {
            return _strategies;
        }

        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        var strategyLogger = loggerFactory.CreateLogger<BaseStrategy>();
        var baseType = typeof(BaseStrategy);
        var strategyTypes = Assembly.GetAssembly(baseType)?.GetTypes() ?? [];

        _strategies = strategyTypes
            .Where(type => type.IsSubclassOf(baseType) && !type.IsAbstract)
            .Select(type => CreateSummary(type, strategyLogger))
            .Where(summary => summary is not null)
            .Cast<StrategySummary>()
            .OrderBy(summary => summary.Kind)
            .ThenBy(summary => summary.Name)
            .ToList();

        return _strategies;
    }

    public BaseStrategy? CreateStrategy(StrategySummary summary)
    {
        var strategyType = Assembly.GetAssembly(typeof(BaseStrategy))?
            .GetType(summary.StrategyTypeName, throwOnError: false);

        if (strategyType is null)
        {
            return null;
        }

        var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
        return Activator.CreateInstance(strategyType, loggerFactory.CreateLogger<BaseStrategy>()) as BaseStrategy;
    }

    private StrategySummary? CreateSummary(Type strategyType, ILogger strategyLogger)
    {
        try
        {
            if (Activator.CreateInstance(strategyType, strategyLogger) is not BaseStrategy strategy)
            {
                return null;
            }

            var isProgrammerMode = typeof(IProgrammerMode).IsAssignableFrom(strategyType);
            return new StrategySummary(
                strategy.StrategyName,
                isProgrammerMode ? "Programmer Mode" : "Preset",
                GetRuntime(strategyType.Name, isProgrammerMode),
                "Available",
                strategyType.FullName ?? strategyType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load strategy metadata for {StrategyType}", strategyType.Name);
            return null;
        }
    }

    private static string GetRuntime(string typeName, bool isProgrammerMode)
    {
        if (!isProgrammerMode)
        {
            return "Native";
        }

        return typeName switch
        {
            "ProgrammerCS" => "C#",
            "ProgrammerJS" => "JavaScript",
            "ProgrammerLUA" => "Lua",
            "ProgrammerPython" => "Python",
            _ => "Script"
        };
    }
}
