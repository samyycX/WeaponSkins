namespace WeaponSkins.Shared;

public record StickerCollectionDefinition
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required IReadOnlyDictionary<string, string> LocalizedNames { get; init; }
    public required IReadOnlyList<StickerDefinition> Stickers { get; init; }
}