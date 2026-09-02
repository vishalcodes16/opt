namespace VishalXOpt.Core.Models;
public sealed class TweakDefinition
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public string Description { get; init; } = "";
    public string Risk { get; init; } = "SAFE";
    public bool RequiresAdmin { get; init; }
    public bool RequiresRestart { get; init; }
    public string? RegistryHive { get; init; }
    public string? RegistryPath { get; init; }
    public string? ValueName { get; init; }
    public object? EnabledValue { get; init; }
    public object? DisabledValue { get; init; }
    public object? RecommendedValue { get; init; }
}
