namespace WeaponSkins.Shared;

public record ItemDefinition
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required Dictionary<string, string> LocalizedNames { get; init; }
}