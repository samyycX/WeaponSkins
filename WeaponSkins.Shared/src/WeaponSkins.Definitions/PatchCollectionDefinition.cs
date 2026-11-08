namespace WeaponSkins.Shared;

public record PatchCollectionDefinition
{
    public required string Name { get; init; }
    public required int Index { get; init; }
    public required Dictionary<string, string> LocalizedNames { get; init; }
    public required List<PatchDefinition> Patches { get; init; }
}
