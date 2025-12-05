namespace WeaponSkins.Shared;

public record StickerCollectionDefinition
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required Dictionary<string, string> LocalizedNames { get; init; }
    public required List<StickerDefinition> Stickers { get; init; }
}