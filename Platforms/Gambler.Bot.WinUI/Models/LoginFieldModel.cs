namespace Gambler.Bot.WinUI.Models;

public sealed class LoginFieldModel
{
    public required string Name { get; init; }
    public bool IsRequired { get; init; }
    public bool IsSecret { get; init; }
    public bool IsMfa { get; init; }
    public string? Value { get; set; }
}
