namespace WeaponSkins.Shared;

public record ItemDefinition
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required IReadOnlyDictionary<string, string> LocalizedNames { get; init; }
}