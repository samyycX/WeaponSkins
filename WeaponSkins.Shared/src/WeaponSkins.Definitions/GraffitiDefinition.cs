namespace WeaponSkins.Shared;

public record GraffitiDefinition
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required Dictionary<string, string> LocalizedNames { get; init; }
    public required RarityDefinition Rarity { get; init; }
    public required bool Tintable { get; init; }
}
