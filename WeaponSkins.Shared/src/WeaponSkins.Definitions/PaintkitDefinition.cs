namespace WeaponSkins.Shared;

public record PaintkitDefinition
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required bool UseLegacyModel { get; init; }
    public required string DescriptionTag { get; init; }
    public required Dictionary<string, string> LocalizedNames { get; init; }
    public required RarityDefinition Rarity { get; init; }
}