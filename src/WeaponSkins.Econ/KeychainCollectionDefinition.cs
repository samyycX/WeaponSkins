using WeaponSkins.Shared;

namespace WeaponSkins.Econ;

// The game's charm collections (its keychain_pack loot lists). Kept off WeaponSkins.Shared,
// like the image keys: only the menu groups charms this way.
public record KeychainCollectionDefinition
{
    public required string Name { get; init; }
    public required Dictionary<string, string> LocalizedNames { get; init; }
    public required List<KeychainDefinition> Keychains { get; init; }
}