namespace WeaponSkins.Shared;

public class MusicKitDefinition
{
    public string Name { get; set; } = string.Empty;
    public int Index { get; set; }
    public Dictionary<string, string> LocalizedNames { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public RarityDefinition Rarity { get; set; } = null!;
}
