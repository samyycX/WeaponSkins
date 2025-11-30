namespace WeaponSkins.Shared;

public record ColorDefinition
{
    public required string Name { get; init; }
    public required string HexColor { get; init; }
}