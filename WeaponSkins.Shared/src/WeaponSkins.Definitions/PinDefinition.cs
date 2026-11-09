namespace WeaponSkins.Shared;

public record PinDefinition
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required Dictionary<string, string> LocalizedNames { get; init; }
    public required RarityDefinition Rarity { get; init; }
}
